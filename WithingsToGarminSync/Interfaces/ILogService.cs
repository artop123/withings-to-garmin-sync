namespace WithingsToGarminSync.Interfaces
{
	public interface ILogService
	{
		public void Log(string message);
		public void Error(string message);
		public void Log(Exception exception);
	}
}
