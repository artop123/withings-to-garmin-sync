using FluentAssertions;
using RestSharp;
using WithingsToGarminSync.Services;
using Xunit;

namespace WithingsToGarminSync.Tests.Services;

public class WithingsHttpClientTests
{
	[Fact]
	public void CreateMeasurementRequest_ShouldUseGetmeasEndpointAndBearerToken()
	{
		var request = WithingsHttpClient.CreateMeasurementRequest("access-token");

		new Uri(new Uri(WithingsHttpClient.ApiBaseUrl), request.Resource)
			.Should().Be(new Uri("https://wbsapi.withings.net/measure"));
		request.Method.Should().Be(Method.Post);
		request.Parameters.Should().ContainSingle(parameter =>
			parameter.Name == "action"
			&& Convert.ToString(parameter.Value) == "getmeas"
			&& parameter.Type == ParameterType.GetOrPost);
		request.Parameters.Should().ContainSingle(parameter =>
			parameter.Name == "Authorization"
			&& Convert.ToString(parameter.Value) == "Bearer access-token"
			&& parameter.Type == ParameterType.HttpHeader);
	}
}
