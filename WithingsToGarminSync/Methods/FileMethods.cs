namespace WithingsToGarminSync.Methods
{
	internal static class FileMethods
	{
		public static void EnsureDirectoryExists(string path)
		{
			var directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}
	}
}
