namespace WithingsToGarminSync.Methods
{
	public static class DateTimeMethods
	{
		public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			// Unix timestamp to DateTime conversion
			DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
			return dateTimeOffset.LocalDateTime;
		}
	}
}
