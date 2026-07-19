using DotnetNiger.Community.Api;

try
{
    var builder = ApplicationSetup.CreateBuilder(args);
    var app = PipelineSetup.ConfigureApp(builder);

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("DotnetNiger.Community starting...");
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger("Program");
    logger.LogCritical(ex, "Application terminated unexpectedly");
    return 1;
}
