using Microsoft.Extensions.Configuration;
using Serilog;
using WithingsToGarminSync.Interfaces;

namespace WithingsToGarminSync.Services;

public class SerilogService : ILogService
{
	public SerilogService(IConfigurationRoot configuration)
	{
		Serilog.Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();
	}

	public void Log(string messageTemplate, params object?[] propertyValues)
	{
		Serilog.Log.Information(messageTemplate, propertyValues);
	}

	public void Error(string messageTemplate, params object?[] propertyValues)
	{
		Serilog.Log.Error(messageTemplate, propertyValues);
	}

	public void Error(Exception exception, string messageTemplate, params object?[] propertyValues)
	{
		Serilog.Log.Error(exception, messageTemplate, propertyValues);
	}
}
