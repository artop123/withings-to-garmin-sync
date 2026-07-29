namespace WithingsToGarminSync.Interfaces;

public interface ILogService
{
	void Log(string messageTemplate, params object?[] propertyValues);
	void Error(string messageTemplate, params object?[] propertyValues);
	void Error(Exception exception, string messageTemplate, params object?[] propertyValues);
}
