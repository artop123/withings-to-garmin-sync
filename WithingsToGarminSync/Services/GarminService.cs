using Garmin.Connect;
using Garmin.Connect.Auth;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Methods;

namespace WithingsToGarminSync.Services;

internal class GarminService : IGarminService
{
	string username = "";
	string password = "";
	string tokenCachePath = "";

	public IGarminService SetUsername(string value)
	{
		username = value;
		return this;
	}

	public IGarminService SetPassword(string value)
	{
		password = value;
		return this;
	}

	public IGarminService SetTokenCachePath(string value)
	{
		tokenCachePath = value;
		return this;
	}

	public async Task<bool> UploadWeight(double weight)
	{
		if (string.IsNullOrWhiteSpace(tokenCachePath))
		{
			throw new Exception("Garmin token cache path is missing.");
		}

		var authParameters = new BasicAuthParameters(username, password);
		FileMethods.EnsureDirectoryExists(tokenCachePath);
		var tokenCache = new FileTokenCache(tokenCachePath);

		var client = new GarminConnectClient(new GarminConnectContext(new HttpClient(), authParameters, null, tokenCache));
		var weightInGrams = weight * 1000;

		await client.SetUserWeight(weightInGrams);

		return true;
	}
}
