using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Services;

namespace WithingsToGarminSync
{
	public class Application
	{
		private readonly string _dataJsonFile = "data.json";
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

		public Application Start(Settings settings)
		{
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

		private TokenInfo GetWithingsToken()
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

			return new TokenInfo()
			{
				AccessToken = token.Access_token,
				AcquiredAt = DateTime.Now,
				ExpiresIn = token.Expires_in,
				RefreshToken = token.Refresh_token
			};
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

			var withingsToken = GetWithingsToken();

			var newToken = _withingsService.GetAccessTokenByRefreshToken(withingsToken.RefreshToken);
			var data = _withingsService.FetchWeightAndFatData(newToken.Access_token);

			if (data == null)
			{
				throw new Exception("Invalid data from Withings");
			}

			var shouldUpdate = _runData == null
				|| _runData.LastWeightDate == null
				|| _runData.LastWeightDate < data.Date;

			_logService?.Log($"Loaded weight from Withings ({data.Weight:0.00} kg, {data.Date})");
			_logService?.Log($"Updating data to Garmin: {shouldUpdate}");

			if (shouldUpdate)
			{
				await _garminService.UploadWeight(data.Weight);
				_logService?.Log("Weight uploaded to Garmin");
			}

			_fileService.SaveRunData(_dataJsonFile, data, withingsToken);
		}
	}
}
