using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Interfaces;

public interface IWithingsHttpClient
{
	WithingsHttpResult<WithingsAccessTokenResponse> RequestAccessToken(
		string clientId,
		string clientSecret,
		string authCode,
		string redirectUri);

	WithingsHttpResult<WithingsAccessTokenResponse> RefreshAccessToken(
		string clientId,
		string clientSecret,
		string refreshToken,
		string redirectUri);

	WithingsHttpResult<WithingsMeasurementResponse> FetchMeasurements(string? accessToken);
}
