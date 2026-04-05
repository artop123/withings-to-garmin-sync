using System.Text.Json;
using FluentAssertions;
using Moq;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Services;
using WithingsToGarminSync.Tests.TestHelpers;
using Xunit;

namespace WithingsToGarminSync.Tests.Services;

public class FileServiceTests
{
	[Fact]
	public void Save_ShouldReturnFalse_WhenPathIsEmpty()
	{
		var service = new FileService(ServiceMockFactory.CreateLogServiceMock().Object);

		var result = service.Save("", new { Value = 1 });

		result.Should().BeFalse();
	}

	[Fact]
	public void Save_ShouldWriteFileAndReturnTrue_WhenInputIsValid()
	{
		var logger = ServiceMockFactory.CreateLogServiceMock();
		var service = new FileService(logger.Object);
		var path = CreateTempPath("save.json");

		try
		{
			var result = service.Save(path, new TestModel { Name = "Alice", Value = 42 });

			result.Should().BeTrue();
			File.Exists(path).Should().BeTrue();

			var loaded = JsonSerializer.Deserialize<TestModel>(File.ReadAllText(path));
			loaded.Should().NotBeNull();
			loaded!.Name.Should().Be("Alice");
			loaded.Value.Should().Be(42);
		}
		finally
		{
			Cleanup(path);
		}
	}

	[Fact]
	public void Load_ShouldReturnDefault_WhenFileDoesNotExist()
	{
		var logger = ServiceMockFactory.CreateLogServiceMock();
		var service = new FileService(logger.Object);
		var path = CreateTempPath("missing.json");

		var result = service.Load<TestModel>(path);

		result.Should().BeNull();
		logger.Verify(x => x.Error(It.Is<string>(s => s.Contains("not found"))), Times.Once);
	}

	[Fact]
	public void Load_ShouldReturnModel_WhenJsonIsValid()
	{
		var service = new FileService(ServiceMockFactory.CreateLogServiceMock().Object);
		var result = service.Load<TestModel>(GetAssetPath("valid.json"));

		result.Should().NotBeNull();
		result!.Name.Should().Be("Bob");
		result.Value.Should().Be(7);
	}

	[Fact]
	public void Load_ShouldReturnDefault_WhenJsonIsInvalid()
	{
		var logger = ServiceMockFactory.CreateLogServiceMock();
		var service = new FileService(logger.Object);
		var result = service.Load<TestModel>(GetAssetPath("invalid.json"));

		result.Should().BeNull();
		logger.Verify(x => x.Log(It.Is<string>(s => s.StartsWith("Failed to load the file:"))), Times.Once);
	}

	[Fact]
	public void SaveRunData_ShouldPersistLatestMeasurementAndToken()
	{
		var service = new FileService(ServiceMockFactory.CreateLogServiceMock().Object);
		var path = CreateTempPath("run-data.json");
		var token = TestDataFactory.CreateToken("access-token", "refresh-token");
		var measurement = TestDataFactory.CreateMeasurement(81.25, new DateTime(2026, 4, 5, 8, 0, 0, DateTimeKind.Utc));

		try
		{
			service.SaveRunData(path, measurement, token);

			var runData = JsonSerializer.Deserialize<RunData>(File.ReadAllText(path));
			runData.Should().NotBeNull();
			runData!.Token.Should().NotBeNull();
			runData.Token!.Access_token.Should().Be("access-token");
			runData.LastWeight.Should().Be(81.25);
			runData.LastWeightDate.Should().Be(measurement.Date);
			runData.LastRun.Should().NotBeNull();
		}
		finally
		{
			Cleanup(path);
		}
	}

	private static string GetAssetPath(string fileName)
	{
		return Path.Combine(AppContext.BaseDirectory, "TestAssets", fileName);
	}

	private static string CreateTempPath(string fileName)
	{
		var directory = Path.Combine(Path.GetTempPath(), "withings-to-garmin-sync-tests", Guid.NewGuid().ToString("N"));
		return Path.Combine(directory, fileName);
	}

	private static void Cleanup(string path)
	{
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
		{
			Directory.Delete(directory, true);
		}
	}

	private sealed class TestModel
	{
		public string Name { get; set; } = "";
		public int Value { get; set; }
	}
}
