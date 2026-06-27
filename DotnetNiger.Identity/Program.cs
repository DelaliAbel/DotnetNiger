using DotnetNiger.Identity.Api;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();

    var app = ApplicationSetup.ConfigureApp(ApplicationSetup.CreateBuilder(args));

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger("Program");
    logger.LogCritical(ex, "Application terminated unexpectedly");
    return 1;
}
