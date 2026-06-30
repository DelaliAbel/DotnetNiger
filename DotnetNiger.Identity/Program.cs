using DotnetNiger.Identity.Api;

try
{
    var builder = ApplicationSetup.CreateBuilder(args);
    var app = ApplicationSetup.ConfigureApp(builder);
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger("Program");
    logger.LogCritical(ex, "Application terminated unexpectedly");
    return 1;
}
