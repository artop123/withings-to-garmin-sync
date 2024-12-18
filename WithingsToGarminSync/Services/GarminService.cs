using Garmin.Connect;
using Garmin.Connect.Auth;

namespace WithingsToGarminSync.Services
{
	internal class GarminService
	{
		string username = "";
		string password = "";

		public GarminService SetUsername(string value)
		{
			username = value;
			return this;
		}

		public GarminService SetPassword(string value)
		{
			password = value;
			return this;
		}

		public async Task<bool> UploadWeight(double weight)
		{
			var authParameters = new BasicAuthParameters(username, password);

			var client = new GarminConnectClient(new GarminConnectContext(new HttpClient(), authParameters));
			var weightInGrams = weight * 1000;

			await client.SetUserWeight(weightInGrams);

			return true;
		}
	}
}
