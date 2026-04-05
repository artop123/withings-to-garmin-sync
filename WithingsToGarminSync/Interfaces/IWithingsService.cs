using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Interfaces;

public interface IWithingsService
{
	IWithingsService SetClientId(string value);
	IWithingsService SetClientSecret(string value);
	IWithingsService SetRedirectUrl(string value);
	string GetAccessCode();
	WithingsAccessTokenBody? GetAccessToken(string authCode);
	WithingsAccessTokenBody? GetAccessTokenByRefreshToken(string? refreshToken);
	bool IsTokenUsable(WithingsAccessTokenBody? token);
	List<MeasurementData> FetchWeightAndFatData(string? accessToken);
}
