namespace WithingsToGarminSync.Interfaces;

public interface IGarminService
{
	IGarminService SetUsername(string value);
	IGarminService SetPassword(string value);
	IGarminService SetTokenCachePath(string value);
	Task<bool> UploadWeight(double weight);
}
