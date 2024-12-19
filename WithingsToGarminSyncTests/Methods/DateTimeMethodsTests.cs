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
			var expectedDateTime = new DateTime(2023, 1, 1, 2, 0, 0, DateTimeKind.Local); // Oletetaan UTC+2 aikavyöhyke

			// Act
			var result = DateTimeMethods.UnixTimeStampToDateTime(unixTimeStamp);

			// Assert
			result.Should().Be(expectedDateTime);
		}
	}
}
