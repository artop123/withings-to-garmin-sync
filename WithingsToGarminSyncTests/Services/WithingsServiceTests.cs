using FluentAssertions;
using Moq;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.Withings;
using WithingsToGarminSync.Services;
using WithingsToGarminSync.Tests.TestHelpers;
using Xunit;

namespace WithingsToGarminSync.Tests.Services;

public class WithingsServiceTests
{
	[Fact]
	public void IsTokenUsable_ShouldReturnFalse_WhenTokenIsNull()
	{
		var service = ServiceMockFactory.CreateWithingsService();

		var result = service.IsTokenUsable(null);

		result.Should().BeFalse();
	}

	[Fact]
	public void IsTokenUsable_ShouldReturnFalse_WhenAccessTokenIsMissing()
	{
		var service = ServiceMockFactory.CreateWithingsService();
		var token = TestDataFactory.CreateToken(accessToken: "", refreshToken: "refresh", expiresInSeconds: 3600, issuedAtUtc: DateTime.UtcNow);

		var result = service.IsTokenUsable(token);

		result.Should().BeFalse();
	}

	[Fact]
	public void IsTokenUsable_ShouldReturnFalse_WhenRefreshTokenIsMissing()
	{
		var service = ServiceMockFactory.CreateWithingsService();
		var token = TestDataFactory.CreateToken(accessToken: "access", refreshToken: "", expiresInSeconds: 3600, issuedAtUtc: DateTime.UtcNow);

		var result = service.IsTokenUsable(token);

		result.Should().BeFalse();
	}

	[Fact]
	public void IsTokenUsable_ShouldReturnFalse_WhenIssuedAtIsMissing()
	{
		var service = ServiceMockFactory.CreateWithingsService();
		var token = TestDataFactory.CreateToken(accessToken: "access", refreshToken: "refresh", expiresInSeconds: 3600, issuedAtUtc: null);

		var result = service.IsTokenUsable(token);

		result.Should().BeFalse();
	}

	[Fact]
	public void IsTokenUsable_ShouldReturnFalse_WhenTokenExpiresInFiveMinutesOrLess()
	{
		var service = ServiceMockFactory.CreateWithingsService();
		var token = TestDataFactory.CreateToken(
			accessToken: "access",
			refreshToken: "refresh",
			expiresInSeconds: 300,
			issuedAtUtc: DateTime.UtcNow);

		var result = service.IsTokenUsable(token);

		result.Should().BeFalse();
	}

	[Fact]
	public void IsTokenUsable_ShouldReturnTrue_WhenTokenExpiresInMoreThanFiveMinutes()
	{
		var service = ServiceMockFactory.CreateWithingsService();
		var token = TestDataFactory.CreateToken(
			accessToken: "access",
			refreshToken: "refresh",
			expiresInSeconds: 301,
			issuedAtUtc: DateTime.UtcNow);

		var result = service.IsTokenUsable(token);

		result.Should().BeTrue();
	}

	[Fact]
	public void GetAccessToken_ShouldReturnToken_WhenHttpResponseIsValid()
	{
		var httpClient = new Mock<IWithingsHttpClient>();
		httpClient
			.Setup(x => x.RequestAccessToken("client-id", "client-secret", "auth-code", "https://localhost/callback"))
			.Returns(new WithingsHttpResult<WithingsAccessTokenResponse>
			{
				IsSuccessful = true,
				Data = new WithingsAccessTokenResponse
				{
					Status = 0,
					Body = new WithingsAccessTokenBody
					{
						Access_token = "access",
						Refresh_token = "refresh",
						Expires_in = 3600
					}
				}
			});

		var service = ServiceMockFactory.CreateWithingsService(httpClient.Object)
			.SetClientId("client-id")
			.SetClientSecret("client-secret")
			.SetRedirectUrl("https://localhost/callback");

		var token = service.GetAccessToken("auth-code");

		token.Should().NotBeNull();
		token!.Access_token.Should().Be("access");
		token.Refresh_token.Should().Be("refresh");
		token.IssuedAtUtc.Should().NotBeNull();
	}

	[Fact]
	public void GetAccessToken_ShouldReturnNull_WhenHttpResponseIsInvalid()
	{
		var httpClient = new Mock<IWithingsHttpClient>();
		httpClient
			.Setup(x => x.RequestAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			.Returns(new WithingsHttpResult<WithingsAccessTokenResponse>
			{
				IsSuccessful = false,
				Content = "error"
			});

		var service = ServiceMockFactory.CreateWithingsService(httpClient.Object);

		var token = service.GetAccessToken("auth-code");

		token.Should().BeNull();
	}

	[Fact]
	public void GetAccessTokenByRefreshToken_ShouldReturnNull_WhenRefreshTokenIsMissing()
	{
		var httpClient = new Mock<IWithingsHttpClient>();
		var service = ServiceMockFactory.CreateWithingsService(httpClient.Object);

		var token = service.GetAccessTokenByRefreshToken("");

		token.Should().BeNull();
		httpClient.Verify(
			x => x.RefreshAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
			Times.Never);
	}

	[Fact]
	public void GetAccessTokenByRefreshToken_ShouldReturnToken_WhenHttpResponseIsValid()
	{
		var httpClient = new Mock<IWithingsHttpClient>();
		httpClient
			.Setup(x => x.RefreshAccessToken("client-id", "client-secret", "refresh-token", "https://localhost/callback"))
			.Returns(new WithingsHttpResult<WithingsAccessTokenResponse>
			{
				IsSuccessful = true,
				Data = new WithingsAccessTokenResponse
				{
					Status = 0,
					Body = new WithingsAccessTokenBody
					{
						Access_token = "access",
						Refresh_token = "refresh-token-2",
						Expires_in = 3600
					}
				}
			});

		var service = ServiceMockFactory.CreateWithingsService(httpClient.Object)
			.SetClientId("client-id")
			.SetClientSecret("client-secret")
			.SetRedirectUrl("https://localhost/callback");

		var token = service.GetAccessTokenByRefreshToken("refresh-token");

		token.Should().NotBeNull();
		token!.Access_token.Should().Be("access");
		token.Refresh_token.Should().Be("refresh-token-2");
		token.IssuedAtUtc.Should().NotBeNull();
	}

	[Fact]
	public void FetchWeightAndFatData_ShouldReturnEmptyList_WhenHttpResponseIsInvalid()
	{
		var httpClient = new Mock<IWithingsHttpClient>();
		httpClient
			.Setup(x => x.FetchMeasurements("access-token"))
			.Returns(new WithingsHttpResult<WithingsMeasurementResponse>
			{
				IsSuccessful = false,
				Content = "error"
			});

		var service = ServiceMockFactory.CreateWithingsService(httpClient.Object);

		var data = service.FetchWeightAndFatData("access-token");

		data.Should().BeEmpty();
	}

	[Fact]
	public void FetchWeightAndFatData_ShouldMapKnownMeasureTypes_WhenHttpResponseIsValid()
	{
		var httpClient = new Mock<IWithingsHttpClient>();
		httpClient
			.Setup(x => x.FetchMeasurements("access-token"))
			.Returns(new WithingsHttpResult<WithingsMeasurementResponse>
			{
				IsSuccessful = true,
				Data = new WithingsMeasurementResponse
				{
					Status = 0,
					Body = new WithingsMeasurementBody
					{
						Measuregrps =
						[
							new WithingsMeasureGroup
							{
								Date = 1672531200,
								Measures =
								[
									new WithingsMeasure { Type = 1, Value = 80500, Unit = -3 },
									new WithingsMeasure { Type = 5, Value = 62000, Unit = -3 },
									new WithingsMeasure { Type = 6, Value = 205, Unit = -1 },
									new WithingsMeasure { Type = 8, Value = 16500, Unit = -3 },
									new WithingsMeasure { Type = 76, Value = 30100, Unit = -3 },
									new WithingsMeasure { Type = 77, Value = 43500, Unit = -3 },
									new WithingsMeasure { Type = 88, Value = 3200, Unit = -3 }
								]
							}
						]
					}
				}
			});

		var service = ServiceMockFactory.CreateWithingsService(httpClient.Object);

		var data = service.FetchWeightAndFatData("access-token");

		data.Should().HaveCount(1);
		data[0].Weight.Should().Be(80.5);
		data[0].FatFreeMass.Should().Be(62.0);
		data[0].FatPercent.Should().Be(20.5);
		data[0].FatMassWeight.Should().Be(16.5);
		data[0].MuscleMass.Should().Be(30.1);
		data[0].Hydration.Should().Be(43.5);
		data[0].BoneMass.Should().Be(3.2);
	}

}
