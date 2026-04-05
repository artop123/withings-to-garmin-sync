using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Methods;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Services;

public class WithingsService : IWithingsService
{
	private readonly ILogService _logService;
	private readonly IWithingsHttpClient _httpClient;
	private const string AuthUrl = "https://account.withings.com/oauth2_user/authorize2";
	string clientId = "";
	string clientSecret = "";
	string redirectUri = "";

	public WithingsService(ILogService logService)
		: this(logService, new WithingsHttpClient())
	{
	}

	public WithingsService(ILogService logService, IWithingsHttpClient httpClient)
	{
		_logService = logService;
		_httpClient = httpClient;
	}

	public IWithingsService SetClientId(string value)
	{
		clientId = value;
		return this;
	}
	public IWithingsService SetClientSecret(string value)
	{
		clientSecret = value;
		return this;
	}
	public IWithingsService SetRedirectUrl(string value)
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
		var response = _httpClient.RequestAccessToken(clientId, clientSecret, authCode, redirectUri);
		if (!response.IsSuccessful
			|| response.Data == null
			|| response.Data.Status != 0
			|| string.IsNullOrWhiteSpace(response.Data.Body.Access_token)
			|| string.IsNullOrWhiteSpace(response.Data.Body.Refresh_token))
		{
			_logService?.Error($"Failed to exchange Withings authorization code for token: {response.Content}");
			return null;
		}

		SetTokenIssuedNow(response.Data.Body);
		return response.Data.Body;
	}

	public WithingsAccessTokenBody? GetAccessTokenByRefreshToken(string? refreshToken)
	{
		if (string.IsNullOrWhiteSpace(refreshToken))
		{
			_logService?.Error("Withings refresh token is missing.");
			return null;
		}

		var response = _httpClient.RefreshAccessToken(clientId, clientSecret, refreshToken, redirectUri);
		if (!response.IsSuccessful
			|| response.Data == null
			|| response.Data.Status != 0
			|| string.IsNullOrWhiteSpace(response.Data.Body.Access_token)
			|| string.IsNullOrWhiteSpace(response.Data.Body.Refresh_token))
		{
			_logService?.Error($"Failed to refresh Withings token: {response.Content}");
			return null;
		}

		SetTokenIssuedNow(response.Data.Body);
		return response.Data.Body;
	}

	public bool IsTokenUsable(WithingsAccessTokenBody? token)
	{
		if (token == null
			|| string.IsNullOrWhiteSpace(token.Access_token)
			|| string.IsNullOrWhiteSpace(token.Refresh_token))
		{
			return false;
		}

		var tokenExpiresAtUtc = token.ExpiresAtUtc;
		if (tokenExpiresAtUtc == null)
		{
			return false;
		}

		// Refresh a few minutes before expiry to avoid failing mid-run.
		return tokenExpiresAtUtc.Value > DateTime.UtcNow.AddMinutes(5);
	}

	private static void SetTokenIssuedNow(WithingsAccessTokenBody token)
	{
		token.IssuedAtUtc = DateTime.UtcNow;
	}

	public List<MeasurementData> FetchWeightAndFatData(string? accessToken)
	{
		var result = new List<MeasurementData>();
		var response = _httpClient.FetchMeasurements(accessToken);

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
				var valueFormatted = measure.Value * Math.Pow(10, measure.Unit);

				switch (measure.Type)
				{
					case 1:
						data.Weight = valueFormatted;
						break;
					case 5:
						data.FatFreeMass = valueFormatted;
						break;
					case 6:
						data.FatPercent = valueFormatted;
						break;
					case 8:
						data.FatMassWeight = valueFormatted;
						break;
					case 76:
						data.MuscleMass = valueFormatted;
						break;
					case 77:
						data.Hydration = valueFormatted;
						break;
					case 88:
						data.BoneMass = valueFormatted;
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
