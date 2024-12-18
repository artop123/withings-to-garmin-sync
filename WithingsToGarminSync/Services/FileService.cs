using System.Text.Json;
using WithingsToGarminSync.Models.General;

namespace WithingsToGarminSync.Services
{
	public class FileService
	{
		public void SaveRunData(string path, MeasurementData? data = null, TokenInfo? newToken = null)
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
		}

		public T? Load<T>(string? path = null)
		{
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
				return default;

			var json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<T>(json);
		}
	}
}
