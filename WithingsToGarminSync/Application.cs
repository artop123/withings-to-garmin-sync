using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;
using WithingsToGarminSync.Services;

namespace WithingsToGarminSync
{
	public class Application
	{
		private readonly string _dataJsonFile = "data.json";
		private readonly string _withingsJsonFile = "withings.json";
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

		private WithingsAccessTokenBody? GetWithingsToken()
		{
			if (_settings == null || _withingsService == null)
			{
				throw new Exception("WithingsServices not found, forgot to Start()?");
			}

			if (_runData?.Token != null)
			{
				_logService?.Log("Using old Withings token");
				return _runData.Token;
			}

			_logService?.Log("Withings token not found, requesting access from the user..");

			var code = _withingsService.GetAccessCode();
			var token = _withingsService.GetAccessToken(code);

			_logService?.Log("Token received..");

			return token;
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

			var token = GetWithingsToken();
			token = _withingsService.GetAccessTokenByRefreshToken(token?.Refresh_token);

			var data = _withingsService.FetchWeightAndFatData(token?.Access_token);

			if (data == null)
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
			_fileService.SaveRunData(_dataJsonFile, latest, token);
		}
	}
}
