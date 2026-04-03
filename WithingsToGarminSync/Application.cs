using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;
using WithingsToGarminSync.Services;

namespace WithingsToGarminSync
{
	public class Application
	{
		private readonly string _dataJsonFile = "data/data.json";
		private readonly string _withingsJsonFile = "data/withings.json";
		private readonly ILogService _logService;

		FileService? _fileService;
		Settings? _settings;
		RunData? _runData;
		WithingsService? _withingsService;
		GarminService? _garminService;

		public Application(ILogService logService)
		{
			_logService = logService;
		}

		public Application Start(Settings? settings)
		{
			ArgumentNullException.ThrowIfNull(settings);
			ArgumentNullException.ThrowIfNull(settings.Garmin);
			ArgumentNullException.ThrowIfNull(settings.Withings);

			_settings = settings;
			_fileService = new FileService(_logService);
			_runData = _fileService.Load<RunData>(_dataJsonFile);

			_withingsService = new WithingsService(_logService)
				.SetClientId(_settings.Withings.ClientId)
				.SetClientSecret(_settings.Withings.ClientSecret)
				.SetRedirectUrl(_settings.Withings.RedirectUrl);

			_garminService = new GarminService()
				.SetUsername(_settings.Garmin.Username)
				.SetPassword(_settings.Garmin.Password);

			return this;
		}

		private WithingsAccessTokenBody GetWithingsToken()
		{
			if (_settings == null || _withingsService == null)
			{
				throw new Exception("WithingsServices not found, forgot to Start()?");
			}

			WithingsAccessTokenBody? token = _runData?.Token;

			if (token != null)
			{
				_logService?.Log("Using old Withings token");
				_logService?.Log("Refreshing Withings token.");

				var refreshedToken = _withingsService.GetAccessTokenByRefreshToken(token.Refresh_token);
				if (HasValidToken(refreshedToken))
				{
					_logService?.Log("Withings token refreshed.");
					return refreshedToken!;
				}

				_logService?.Error("Withings refresh token is no longer valid. Re-authorization required.");
			}

			token = RequestNewWithingsToken();
			_logService?.Log("Token received..");
			return token!;
		}

		private static bool HasValidToken(WithingsAccessTokenBody? token)
		{
			return token != null
				&& !string.IsNullOrWhiteSpace(token.Access_token)
				&& !string.IsNullOrWhiteSpace(token.Refresh_token);
		}

		private WithingsAccessTokenBody RequestNewWithingsToken()
		{
			if (_withingsService == null)
			{
				throw new Exception("WithingsServices not found, forgot to Start()?");
			}

			if (Console.IsInputRedirected)
			{
				throw new Exception("Withings token request failed in non-interactive mode. Run the app once interactively to authorize Withings.");
			}

			_logService?.Log("Requesting a new Withings authorization from the user.");
			var code = _withingsService.GetAccessCode();
			var token = _withingsService.GetAccessToken(code);

			if (!HasValidToken(token))
			{
				throw new Exception("Unable to acquire a valid Withings token.");
			}

			return token!;
		}

		public async Task Run()
		{
			if (_settings == null
				|| _garminService == null
				|| _withingsService == null
				|| _fileService == null)
			{
				throw new Exception("Services not found, forgot to Start()?");
			}

			var currentToken = GetWithingsToken();

			var data = _withingsService.FetchWeightAndFatData(currentToken.Access_token);

			if (data == null || data.Count == 0)
			{
				throw new Exception("Invalid data from Withings");
			}

			var latest = data
				.OrderByDescending(d => d.Date)
				.First();

			var shouldUpdate = _runData == null
				|| _runData.LastWeightDate == null
				|| _runData.LastWeightDate < latest.Date;

			_logService?.Log($"Loaded weight from Withings ({latest.Weight:0.00} kg, {latest.Date})");
			_logService?.Log($"Updating data to Garmin: {shouldUpdate}");

			if (shouldUpdate)
			{
				await _garminService.UploadWeight(latest.Weight);
				_logService?.Log("Weight uploaded to Garmin");
			}

			_fileService.Save(_withingsJsonFile, data);
			_fileService.SaveRunData(_dataJsonFile, latest, currentToken);
		}
	}
}
