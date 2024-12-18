namespace WithingsToGarminSync.Models.Withings
{
	class WithingsMeasurementBody
	{
		public long UpdateTime { get; set; }
		public string Timezone { get; set; }
		public List<WithingsMeasureGroup> Measuregrps { get; set; }
		public int More { get; set; }
		public int Offset { get; set; }
	}
}
