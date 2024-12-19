namespace WithingsToGarminSync.Methods
{
	public static class DateTimeMethods
	{
		public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
			return dateTimeOffset.LocalDateTime;
		}
	}
}
