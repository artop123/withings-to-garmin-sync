using WithingsToGarminSync.Models.Withings;

namespace WithingsToGarminSync.Models.General
{
	public class RunData
	{
		public WithingsAccessTokenBody? Token { get; set; }
		public DateTime? LastRun { get; set; }
		public double LastWeight { get; set; }
		public DateTime? LastWeightDate { get; set; }
	}
}
