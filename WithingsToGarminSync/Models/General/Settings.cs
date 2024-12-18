namespace WithingsToGarminSync.Models.General
{
	public class WithingsSettings
	{
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string RedirectUrl { get; set; }
	}

	public class GarminSettings
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class Settings
	{
		public WithingsSettings Withings { get; set; }
		public GarminSettings Garmin { get; set; }
	}

	public class RunData
	{
		public TokenInfo Token { get; set; }
		public DateTime? LastRun { get; set; }
		public double LastWeight { get; set; }
		public DateTime? LastWeightDate { get; set; }
	}
}
