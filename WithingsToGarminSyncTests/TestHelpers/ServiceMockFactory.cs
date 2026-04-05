using Moq;
using WithingsToGarminSync.Interfaces;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Services;

namespace WithingsToGarminSync.Tests.TestHelpers;

internal static class ServiceMockFactory
{
	public static Mock<IFileService> CreateFileServiceMock(RunData? runData)
	{
		var mock = new Mock<IFileService>();
		mock.Setup(x => x.Load<RunData>("data/data.json")).Returns(runData);
		mock.Setup(x => x.Save(It.IsAny<string>(), It.IsAny<object>())).Returns(true);
		return mock;
	}

	public static Mock<IWithingsService> CreateWithingsServiceMock()
	{
		var mock = new Mock<IWithingsService>();
		mock.Setup(x => x.SetClientId(It.IsAny<string>())).Returns(mock.Object);
		mock.Setup(x => x.SetClientSecret(It.IsAny<string>())).Returns(mock.Object);
		mock.Setup(x => x.SetRedirectUrl(It.IsAny<string>())).Returns(mock.Object);
		return mock;
	}

	public static Mock<IGarminService> CreateGarminServiceMock()
	{
		var mock = new Mock<IGarminService>();
		mock.Setup(x => x.SetUsername(It.IsAny<string>())).Returns(mock.Object);
		mock.Setup(x => x.SetPassword(It.IsAny<string>())).Returns(mock.Object);
		mock.Setup(x => x.SetTokenCachePath(It.IsAny<string>())).Returns(mock.Object);
		return mock;
	}

	public static Mock<ILogService> CreateLogServiceMock()
	{
		var mock = new Mock<ILogService>();
		mock.Setup(x => x.Log(It.IsAny<string>()));
		mock.Setup(x => x.Error(It.IsAny<string>()));
		mock.Setup(x => x.Log(It.IsAny<Exception>()));
		return mock;
	}

	public static WithingsService CreateWithingsService(IWithingsHttpClient? httpClient = null)
	{
		return httpClient == null
			? new WithingsService(CreateLogServiceMock().Object)
			: new WithingsService(CreateLogServiceMock().Object, httpClient);
	}
}
