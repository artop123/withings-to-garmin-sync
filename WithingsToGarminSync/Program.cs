using Microsoft.Extensions.Configuration;
using WithingsToGarminSync;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Services;

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: false)
	.AddEnvironmentVariables()
	.Build();

var logger = new SerilogService(configuration);
var settings = configuration.Get<Settings>();

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
	if (eventArgs.ExceptionObject is Exception exception
		&& !string.IsNullOrWhiteSpace(exception.Message))
	{
		logger.Error(exception, "Unhandled exception");
	}
};

try
{
	await new Application(logger)
		.Start(settings)
		.Run();
}
catch (Exception exception)
{
	logger.Error(exception, "Unhandled exception");
	throw;
}
finally
{
	await Serilog.Log.CloseAndFlushAsync();
}
