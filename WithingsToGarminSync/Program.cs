using Microsoft.Extensions.Configuration;
using WithingsToGarminSync;
using WithingsToGarminSync.Models.General;
using WithingsToGarminSync.Services;

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: false)
	.Build();

var logger = new SerilogService(configuration);
var settings = configuration.Get<Settings>();

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
	if (eventArgs.ExceptionObject is Exception exception
		&& !string.IsNullOrWhiteSpace(exception.Message))
	{
		logger.Log(exception);
	}
};

await new Application(logger)
	.Start(settings)
	.Run();
