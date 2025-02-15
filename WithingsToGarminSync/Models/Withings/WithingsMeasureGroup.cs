namespace WithingsToGarminSync.Models.Withings
{
	class WithingsMeasureGroup
	{
		public long GrpId { get; set; }
		public int Attrib { get; set; }
		public long Date { get; set; }
		public long Created { get; set; }
		public long Modified { get; set; }
		public int Category { get; set; }
		public string DeviceId { get; set; } = "";
		public string HashDeviceId { get; set; } = "";
		public List<WithingsMeasure> Measures { get; set; } = [];
		public string Comment { get; set; } = "";
		public string Timezone { get; set; } = "";
	}
}
