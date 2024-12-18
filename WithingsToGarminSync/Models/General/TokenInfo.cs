namespace WithingsToGarminSync.Models.General
{
	public class TokenInfo
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public int ExpiresIn { get; set; }
		public DateTime AcquiredAt { get; set; }
	}
}
