using RestSharp;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Services;

public class WithingsHttpClient : IWithingsHttpClient
{
	internal const string ApiBaseUrl = "https://wbsapi.withings.net/";

	public WithingsHttpResult<WithingsAccessTokenResponse> RequestAccessToken(
		string clientId,
		string clientSecret,
		string authCode,
		string redirectUri)
	{
		var client = new RestClient(ApiBaseUrl);
		var request = new RestRequest("v2/oauth2", Method.Post);
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
		var client = new RestClient(ApiBaseUrl);
		var request = new RestRequest("v2/oauth2", Method.Post);
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
		var client = new RestClient(ApiBaseUrl);
		var request = CreateMeasurementRequest(accessToken);

		var response = client.Execute<WithingsMeasurementResponse>(request);
		return new WithingsHttpResult<WithingsMeasurementResponse>
		{
			IsSuccessful = response.IsSuccessful,
			Data = response.Data,
			Content = response.Content
		};
	}

	internal static RestRequest CreateMeasurementRequest(string? accessToken)
	{
		var request = new RestRequest("measure", Method.Post);
		request.AddParameter("action", "getmeas");
		request.AddHeader("Authorization", $"Bearer {accessToken}");
		return request;
	}
}
