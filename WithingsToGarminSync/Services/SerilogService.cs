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

	public void Log(string message)
	{
		Serilog.Log.Information(message);
	}

	public void Log(Exception exception)
	{
		if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
		{
			Serilog.Log.Error(exception, "{ExceptionMessage}", exception.Message);
		}
	}

	public void Error(string message)
	{
		Serilog.Log.Error(message);
	}
}
