using FluentAssertions;
using Moq;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Tests.TestHelpers;
using Xunit;

namespace WithingsToGarminSync.Tests.Application;

public class ApplicationTests
{
	[Fact]
	public async Task Run_ShouldUseExistingToken_WhenTokenIsUsable()
	{
		var existingToken = TestDataFactory.CreateToken("existing-token", "refresh-token");
		var runData = new RunData { Token = existingToken };
		var measurements = new List<MeasurementData> { TestDataFactory.CreateMeasurement(80.5, DateTime.UtcNow) };

		var fileService = ServiceMockFactory.CreateFileServiceMock(runData);
		var withingsService = ServiceMockFactory.CreateWithingsServiceMock();
		var garminService = ServiceMockFactory.CreateGarminServiceMock();
		var logService = ServiceMockFactory.CreateLogServiceMock();

		withingsService.Setup(x => x.IsTokenUsable(existingToken)).Returns(true);
		withingsService.Setup(x => x.FetchWeightAndFatData(existingToken.Access_token)).Returns(measurements);
		garminService.Setup(x => x.UploadWeight(It.IsAny<double>())).ReturnsAsync(true);

		var app = CreateApp(logService, fileService, withingsService, garminService);

		await app.Start(TestDataFactory.CreateSettings()).Run();

		withingsService.Verify(x => x.GetAccessTokenByRefreshToken(It.IsAny<string>()), Times.Never);
		withingsService.Verify(x => x.GetAccessCode(), Times.Never);
		withingsService.Verify(x => x.GetAccessToken(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task Run_ShouldRefreshToken_WhenExistingTokenIsExpired()
	{
		var existingToken = TestDataFactory.CreateToken("expired-token", "refresh-token");
		var refreshedToken = TestDataFactory.CreateToken("refreshed-token", "refresh-token-2");
		var runData = new RunData { Token = existingToken };
		var measurements = new List<MeasurementData> { TestDataFactory.CreateMeasurement(81.2, DateTime.UtcNow) };

		var fileService = ServiceMockFactory.CreateFileServiceMock(runData);
		var withingsService = ServiceMockFactory.CreateWithingsServiceMock();
		var garminService = ServiceMockFactory.CreateGarminServiceMock();
		var logService = ServiceMockFactory.CreateLogServiceMock();

		withingsService.Setup(x => x.IsTokenUsable(existingToken)).Returns(false);
		withingsService.Setup(x => x.GetAccessTokenByRefreshToken(existingToken.Refresh_token)).Returns(refreshedToken);
		withingsService.Setup(x => x.IsTokenUsable(refreshedToken)).Returns(true);
		withingsService.Setup(x => x.FetchWeightAndFatData(refreshedToken.Access_token)).Returns(measurements);
		garminService.Setup(x => x.UploadWeight(It.IsAny<double>())).ReturnsAsync(true);

		var app = CreateApp(logService, fileService, withingsService, garminService);

		await app.Start(TestDataFactory.CreateSettings()).Run();

		withingsService.Verify(x => x.GetAccessTokenByRefreshToken(existingToken.Refresh_token), Times.Once);
		withingsService.Verify(x => x.GetAccessCode(), Times.Never);
		withingsService.Verify(x => x.FetchWeightAndFatData(refreshedToken.Access_token), Times.Once);
	}

	[Fact]
	public async Task Run_ShouldUploadWeight_WhenLatestMeasurementIsNewer()
	{
		var existingToken = TestDataFactory.CreateToken("existing-token", "refresh-token");
		var latestDate = DateTime.UtcNow;
		var runData = new RunData
		{
			Token = existingToken,
			LastWeightDate = latestDate.AddDays(-1)
		};

		var measurements = new List<MeasurementData>
		{
			TestDataFactory.CreateMeasurement(79.9, latestDate.AddDays(-2)),
			TestDataFactory.CreateMeasurement(80.1, latestDate)
		};

		var fileService = ServiceMockFactory.CreateFileServiceMock(runData);
		var withingsService = ServiceMockFactory.CreateWithingsServiceMock();
		var garminService = ServiceMockFactory.CreateGarminServiceMock();
		var logService = ServiceMockFactory.CreateLogServiceMock();

		withingsService.Setup(x => x.IsTokenUsable(existingToken)).Returns(true);
		withingsService.Setup(x => x.FetchWeightAndFatData(existingToken.Access_token)).Returns(measurements);
		garminService.Setup(x => x.UploadWeight(80.1)).ReturnsAsync(true);

		var app = CreateApp(logService, fileService, withingsService, garminService);

		await app.Start(TestDataFactory.CreateSettings()).Run();

		garminService.Verify(x => x.UploadWeight(80.1), Times.Once);
		fileService.Verify(x => x.Save("data/withings.json", measurements), Times.Once);
		fileService.Verify(
			x => x.SaveRunData("data/data.json", It.Is<MeasurementData>(m => m.Weight == 80.1), existingToken),
			Times.Once);
	}

	[Fact]
	public async Task Run_ShouldNotUploadWeight_WhenLatestMeasurementIsNotNewer()
	{
		var existingToken = TestDataFactory.CreateToken("existing-token", "refresh-token");
		var latestDate = DateTime.UtcNow;
		var runData = new RunData
		{
			Token = existingToken,
			LastWeightDate = latestDate
		};

		var measurements = new List<MeasurementData> { TestDataFactory.CreateMeasurement(80.1, latestDate) };

		var fileService = ServiceMockFactory.CreateFileServiceMock(runData);
		var withingsService = ServiceMockFactory.CreateWithingsServiceMock();
		var garminService = ServiceMockFactory.CreateGarminServiceMock();
		var logService = ServiceMockFactory.CreateLogServiceMock();

		withingsService.Setup(x => x.IsTokenUsable(existingToken)).Returns(true);
		withingsService.Setup(x => x.FetchWeightAndFatData(existingToken.Access_token)).Returns(measurements);

		var app = CreateApp(logService, fileService, withingsService, garminService);

		await app.Start(TestDataFactory.CreateSettings()).Run();

		garminService.Verify(x => x.UploadWeight(It.IsAny<double>()), Times.Never);
	}

	[Fact]
	public async Task Run_ShouldThrow_WhenWithingsReturnsNoData()
	{
		var existingToken = TestDataFactory.CreateToken("existing-token", "refresh-token");
		var runData = new RunData { Token = existingToken };

		var fileService = ServiceMockFactory.CreateFileServiceMock(runData);
		var withingsService = ServiceMockFactory.CreateWithingsServiceMock();
		var garminService = ServiceMockFactory.CreateGarminServiceMock();
		var logService = ServiceMockFactory.CreateLogServiceMock();

		withingsService.Setup(x => x.IsTokenUsable(existingToken)).Returns(true);
		withingsService.Setup(x => x.FetchWeightAndFatData(existingToken.Access_token)).Returns(new List<MeasurementData>());

		var app = CreateApp(logService, fileService, withingsService, garminService);

		var act = async () => await app.Start(TestDataFactory.CreateSettings()).Run();

		await act.Should().ThrowAsync<Exception>().WithMessage("Invalid data from Withings");
	}

	private static WithingsToGarminSync.Application CreateApp(
		Mock<ILogService> logService,
		Mock<IFileService> fileService,
		Mock<IWithingsService> withingsService,
		Mock<IGarminService> garminService)
	{
		return new WithingsToGarminSync.Application(
			logService.Object,
			fileService.Object,
			withingsService.Object,
			garminService.Object);
	}
}
