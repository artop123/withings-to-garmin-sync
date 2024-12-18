using Microsoft.Extensions.Configuration;
using Serilog;
using WithingsToGarminSync;
using WithingsToGarminSync.Models.General;

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
	var exception = eventArgs.ExceptionObject as Exception;

	if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
	{
		Log.Error(exception.Message);
	}
};

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: false)
	.Build();

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(configuration)
	.CreateLogger();

var settings = configuration.Get<Settings>();

await new Application()
	.Start(settings)
	.Run();
