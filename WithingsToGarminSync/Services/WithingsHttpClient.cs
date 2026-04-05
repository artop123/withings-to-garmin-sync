using RestSharp;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Services;

public class WithingsHttpClient : IWithingsHttpClient
{
	private const string BaseUrl = "https://wbsapi.withings.net/v2/";
	private const string MeasureUrl = "https://wbsapi.withings.net/measure";

	public WithingsHttpResult<WithingsAccessTokenResponse> RequestAccessToken(
		string clientId,
		string clientSecret,
		string authCode,
		string redirectUri)
	{
		var client = new RestClient(BaseUrl);
		var request = new RestRequest("oauth2", Method.Post);
		request.AddParameter("action", "requesttoken");
		request.AddParameter("grant_type", "authorization_code");
		request.AddParameter("client_id", clientId);
		request.AddParameter("client_secret", clientSecret);
		request.AddParameter("code", authCode);
		request.AddParameter("redirect_uri", redirectUri);

		var response = client.Execute<WithingsAccessTokenResponse>(request);
		return new WithingsHttpResult<WithingsAccessTokenResponse>
		{
			IsSuccessful = response.IsSuccessful,
			Data = response.Data,
			Content = response.Content
		};
	}

	public WithingsHttpResult<WithingsAccessTokenResponse> RefreshAccessToken(
		string clientId,
		string clientSecret,
		string refreshToken,
		string redirectUri)
	{
		var client = new RestClient(BaseUrl);
		var request = new RestRequest("oauth2", Method.Post);
		request.AddParameter("action", "requesttoken");
		request.AddParameter("grant_type", "refresh_token");
		request.AddParameter("client_id", clientId);
		request.AddParameter("client_secret", clientSecret);
		request.AddParameter("refresh_token", refreshToken);
		request.AddParameter("redirect_uri", redirectUri);

		var response = client.Execute<WithingsAccessTokenResponse>(request);
		return new WithingsHttpResult<WithingsAccessTokenResponse>
		{
			IsSuccessful = response.IsSuccessful,
			Data = response.Data,
			Content = response.Content
		};
	}

	public WithingsHttpResult<WithingsMeasurementResponse> FetchMeasurements(string? accessToken)
	{
		var client = new RestClient(MeasureUrl);
		var request = new RestRequest("v2/measure", Method.Get);
		request.AddParameter("action", "getmeas");
		request.AddParameter("access_token", accessToken);

		var response = client.Execute<WithingsMeasurementResponse>(request);
		return new WithingsHttpResult<WithingsMeasurementResponse>
		{
			IsSuccessful = response.IsSuccessful,
			Data = response.Data,
			Content = response.Content
		};
	}
}
