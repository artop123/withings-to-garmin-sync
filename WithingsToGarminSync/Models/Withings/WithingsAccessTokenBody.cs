using System.Text.Json.Serialization;

namespace WithingsToGarminSync.Models.Withings;

public class WithingsAccessTokenBody
{
	public int UserId { get; set; }
	public string Access_token { get; set; } = "";
	public string Refresh_token { get; set; } = "";
	public int Expires_in { get; set; }
	public DateTime? IssuedAtUtc { get; set; }

	[JsonIgnore]
	public DateTime? ExpiresAtUtc => IssuedAtUtc?.AddSeconds(Expires_in > 0 ? Expires_in : 0);

	public string Scope { get; set; } = "";
	public string Token_type { get; set; } = "";
}
