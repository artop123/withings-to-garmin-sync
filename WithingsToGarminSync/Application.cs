using Serilog;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Services;

namespace WithingsToGarminSync
{
	public class Application
	{
		private readonly string _dataJsonFile = "data.json";

		FileService? _fileService;
		Settings? _settings;
		RunData? _runData;
		WithingsService? _withingsService;
		GarminService? _garminService;

		public Application Start(Settings settings)
		{
			_settings = settings;
			_fileService = new FileService();
			_runData = _fileService.Load<RunData>(_dataJsonFile);

			_withingsService = new WithingsService()
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
				Log.Information("Using old Withings token");
				return _runData.Token;
			}

			Log.Information("Withings token not found, requesting access from the user..");

			var code = _withingsService.GetAccessCode();
			var token = _withingsService.GetAccessToken(code);

			Log.Information("Token received..");

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
			var shouldUpdate = _runData == null
				|| _runData.LastWeightDate == null
				|| _runData.LastWeightDate < data.Date;

			Log.Information($"Loaded weight from Withings ({data.Weight:0.00} kg, {data.Date})");
			Log.Information($"Updating data to Garmin: {shouldUpdate}");

			if (shouldUpdate)
			{
				await _garminService.UploadWeight(data.Weight);
				Log.Information("Weight uploaded to Garmin");
			}

			_fileService.SaveRunData(_dataJsonFile, data, withingsToken);
		}
	}
}
