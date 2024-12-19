using FluentAssertions;
using Xunit;

namespace WithingsToGarminSync.Methods.Tests
{
	public class DateTimeMethodsTests
	{
		[Fact]
		public void UnixTimeStampToDateTime_ShouldConvertUnixTimeStampToCorrectDateTime()
		{
			// Arrange
			long unixTimeStamp = 1672531200; // 1. tammikuuta 2023, 00:00:00 UTC
			var expectedDateTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			// Act
			var result = DateTimeMethods.UnixTimeStampToDateTime(unixTimeStamp);
			var resultAsUtc = result.ToUniversalTime();

			// Assert
			resultAsUtc.Should().Be(expectedDateTime);
		}
	}
}
