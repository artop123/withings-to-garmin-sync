using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Interfaces;

public interface IFileService
{
	void SaveRunData(string path, MeasurementData? data = null, WithingsAccessTokenBody? newToken = null);
	T? Load<T>(string? path = null);
	bool Save(string path, object model);
}
