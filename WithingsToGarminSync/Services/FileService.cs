using System.Text.Json;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Services
{
	public class FileService
	{
		private readonly ILogService _logService;

		public FileService(ILogService logService)
		{
			_logService = logService;
		}

		public void SaveRunData(string path, MeasurementData? data = null, WithingsAccessTokenBody? newToken = null)
		{
			var model = new RunData()
			{
				LastRun = DateTime.Now
			};

			if (data != null)
			{
				model.LastWeightDate = data.Date;
				model.LastWeight = data.Weight;
			}

			if (newToken != null)
			{
				model.Token = newToken;
			}

			var json = JsonSerializer.Serialize(model);
			File.WriteAllText(path, json);

			_logService?.Log($"File {path} saved");
		}

		public T? Load<T>(string? path = null)
		{
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			{
				_logService?.Error($"File {path} not found");
				return default;
			}

			_logService?.Log($"Loading {typeof(T)} from {path}");

			try {
				var json = File.ReadAllText(path);
				return JsonSerializer.Deserialize<T>(json);
			}

			catch (Exception ex)
			{
				_logService?.Log($"Failed to load the file: {ex.Message}");
				return default;
			}
		}
	}
}
