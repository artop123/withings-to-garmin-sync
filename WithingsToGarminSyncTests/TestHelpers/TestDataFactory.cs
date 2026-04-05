using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Tests.TestHelpers;

internal static class TestDataFactory
{
	public static Settings CreateSettings()
	{
		return new Settings
		{
			Withings = new WithingsSettings
			{
				ClientId = "client-id",
				ClientSecret = "client-secret",
				RedirectUrl = "https://localhost/callback"
			},
			Garmin = new GarminSettings
			{
				Username = "username",
				Password = "password"
			}
		};
	}

	public static WithingsAccessTokenBody CreateToken(string accessToken, string refreshToken)
	{
		return new WithingsAccessTokenBody
		{
			Access_token = accessToken,
			Refresh_token = refreshToken,
			Expires_in = 3600,
			IssuedAtUtc = DateTime.UtcNow
		};
	}

	public static WithingsAccessTokenBody CreateToken(
		string accessToken,
		string refreshToken,
		int expiresInSeconds,
		DateTime? issuedAtUtc)
	{
		return new WithingsAccessTokenBody
		{
			Access_token = accessToken,
			Refresh_token = refreshToken,
			Expires_in = expiresInSeconds,
			IssuedAtUtc = issuedAtUtc
		};
	}

	public static MeasurementData CreateMeasurement(double weight, DateTime date)
	{
		return new MeasurementData
		{
			Weight = weight,
			Date = date
		};
	}
}
