using RestSharp;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Methods;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Services
{
	public class WithingsService
	{
		private readonly ILogService _logService;
		private const string BaseUrl = "https://wbsapi.withings.net/v2/";
		private const string MeasureUrl = "https://wbsapi.withings.net/measure";
		private const string AuthUrl = "https://account.withings.com/oauth2_user/authorize2";
		string clientId = "";
		string clientSecret = "";
		string redirectUri = "";

		public WithingsService(ILogService logService)
		{
			_logService = logService;
		}

		public WithingsService SetClientId(string value)
		{
			clientId = value;
			return this;
		}
		public WithingsService SetClientSecret(string value)
		{
			clientSecret = value;
			return this;
		}
		public WithingsService SetRedirectUrl(string value)
		{
			redirectUri = value;
			return this;
		}

		public string GetAccessCode()
		{
			Console.WriteLine("Open the following URL in the browser to grant Withings access:");
			string authUrl = $"{AuthUrl}?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope=user.metrics&state=1234";
			Console.WriteLine(authUrl);

			Console.Write("Enter the 'code' parameter from the redirect URL: ");
			var code = Console.ReadLine();

			return code ?? "";
		}

		public WithingsAccessTokenBody? GetAccessToken(string authCode)
		{
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("oauth2", Method.Post);
			request.AddParameter("action", "requesttoken");
			request.AddParameter("grant_type", "authorization_code");
			request.AddParameter("client_id", clientId);
			request.AddParameter("client_secret", clientSecret);
			request.AddParameter("code", authCode);
			request.AddParameter("redirect_uri", redirectUri);

			var response = client.Execute<WithingsAccessTokenResponse>(request);

			return response.Data?.Body;
		}

		public WithingsAccessTokenBody? GetAccessTokenByRefreshToken(string? refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
				return null;

			var client = new RestClient(BaseUrl);
			var request = new RestRequest("oauth2", Method.Post);
			request.AddParameter("action", "requesttoken");
			request.AddParameter("grant_type", "refresh_token");
			request.AddParameter("client_id", clientId);
			request.AddParameter("client_secret", clientSecret);
			request.AddParameter("refresh_token", refreshToken);
			request.AddParameter("redirect_uri", redirectUri);

			var response = client.Execute<WithingsAccessTokenResponse>(request);

			return response.Data?.Body;
		}

		public List<MeasurementData> FetchWeightAndFatData(string? accessToken)
		{
			var client = new RestClient(MeasureUrl);
			var request = new RestRequest("v2/measure", Method.Get);
			var result = new List<MeasurementData>();

			request.AddParameter("action", "getmeas");
			request.AddParameter("access_token", accessToken);

			var response = client.Execute<WithingsMeasurementResponse>(request);

			if (!response.IsSuccessful
				|| response.Data == null
				|| response.Data.Status != 0
				|| response.Data?.Body?.Measuregrps == null)
			{
				_logService?.Error($"Unable to load data from Withings: {response.Content}");
				return result;
			}

			foreach (var group in response.Data.Body.Measuregrps)
			{
				var data = new MeasurementData()
				{
					Date = DateTimeMethods.UnixTimeStampToDateTime(group.Date)
				};

				foreach (var measure in group.Measures)
				{
					// https://developer.withings.com/api-reference/#tag/measure/operation/measure-getmeas
					switch (measure.Type)
					{
						case 1:
							data.Weight = measure.Value * Math.Pow(10, measure.Unit);
							break;
						case 5:
							data.FatFreeMass = measure.Value * Math.Pow(10, measure.Unit);
							break;
						case 6:
							data.FatPercent = measure.Value * Math.Pow(10, measure.Unit);
							break;
						case 8:
							data.FatMassWeight = measure.Value * Math.Pow(10, measure.Unit);
							break;
						case 76:
							data.MuscleMass = measure.Value * Math.Pow(10, measure.Unit);
							break;
						case 77:
							data.Hydration = measure.Value * Math.Pow(10, measure.Unit);
							break;
						case 88:
							data.BoneMass = measure.Value * Math.Pow(10, measure.Unit);
							break;
						default:
							break;
					}
				}

				result.Add(data);
			}

			return result;
		}
	}
}
