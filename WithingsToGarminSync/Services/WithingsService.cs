using RestSharp;
using Serilog;
using WithingsToGarminSync.Methods;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Services
{
	public class WithingsService
	{
		private const string BaseUrl = "https://wbsapi.withings.net/v2/";
		private const string MeasureUrl = "https://wbsapi.withings.net/measure";
		private const string AuthUrl = "https://account.withings.com/oauth2_user/authorize2";
		string clientId;
		string clientSecret;
		string redirectUri;

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

			return code;
		}

		public WithingsAccessTokenBody GetAccessToken(string authCode)
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

			return response.Data.Body;
		}

		public WithingsAccessTokenBody GetAccessTokenByRefreshToken(string refreshToken)
		{
			var client = new RestClient(BaseUrl);
			var request = new RestRequest("oauth2", Method.Post);
			request.AddParameter("action", "requesttoken");
			request.AddParameter("grant_type", "refresh_token");
			request.AddParameter("client_id", clientId);
			request.AddParameter("client_secret", clientSecret);
			request.AddParameter("refresh_token", refreshToken);
			request.AddParameter("redirect_uri", redirectUri);

			var response = client.Execute<WithingsAccessTokenResponse>(request);

			return response.Data.Body;
		}

		public MeasurementData? FetchWeightAndFatData(string accessToken)
		{
			var client = new RestClient(MeasureUrl);
			var request = new RestRequest("v2/measure", Method.Get);
			request.AddParameter("action", "getmeas");
			request.AddParameter("access_token", accessToken);

			var response = client.Execute<WithingsMeasurementResponse>(request);

			if (!response.IsSuccessful || response.Data == null || response.Data.Status != 0)
			{
				Log.Error($"Unable to load data from Withings: {response.Content}");
				return null;
			}

			var latest = response.Data.Body.Measuregrps
				.OrderByDescending(m => m.Date)
				.FirstOrDefault();

			if (latest == null)
				return null;

			var result = new MeasurementData()
			{
				Date = DateTimeMethods.UnixTimeStampToDateTime(latest.Date)
			};

			foreach (var measure in latest.Measures)
			{
				switch (measure.Type)
				{
					case 1: // Paino
						result.Weight = measure.Value * Math.Pow(10, measure.Unit);
						break;
					case 5: // Fat Free Mass (kg)
						result.FatFreeMass = measure.Value * Math.Pow(10, measure.Unit);
						break;
					case 6: // Rasvaprosentti
						result.FatPercent = measure.Value * Math.Pow(10, measure.Unit);
						break;
					case 8: // Fat Mass Weight (kg)
						result.FatMassWeight = measure.Value * Math.Pow(10, measure.Unit);
						break;
					case 76: // Muscle Mass (kg)
						result.MuscleMass = measure.Value * Math.Pow(10, measure.Unit);
						break;
					case 77: // Hydration  (kg)
						result.Hydration = measure.Value * Math.Pow(10, measure.Unit);
						break;
					case 88: // Bone Mass  (kg)
						result.BoneMass = measure.Value * Math.Pow(10, measure.Unit);
						break;
					default:
						break;
				}
			}

			return result;
		}
	}
}
