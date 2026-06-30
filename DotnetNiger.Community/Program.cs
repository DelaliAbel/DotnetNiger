using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetNiger.Community.Api;
using DotnetNiger.Community.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

    builder.Services.AddProblemDetails();
    builder.Services.AddApiVersioningWithSwagger();
    builder.Services.AddCommunityInfrastructure(builder.Configuration);
    builder.Services.AddCommunityAuthentication(builder.Configuration, builder.Environment);
    builder.Services.AddCommunityServices();
    builder.Services.AddCommunityHttpClients(builder.Configuration);
    builder.Services.AddCommunityCors(builder.Configuration);
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.MapOpenApi();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapGet("/swagger", async (HttpContext ctx) =>
        {
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.WriteAsync(SwaggerUiPage("/openapi/v1.json", "DotnetNiger Community API v1"));
        });
    }

    app.UseStaticFiles();
    app.UseCors("AllowFrontendOrigins");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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

static string SwaggerUiPage(string specUrl, string title)
{
    return $$"""
<!DOCTYPE html>
<html lang="en">
<head><meta charset="utf-8"/><title>{{title}}</title>
<link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
</head>
<body>
<div id="swagger-ui"></div>
<script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
<script>
SwaggerUIBundle({ url: '{{specUrl}}', dom_id: '#swagger-ui', presets: [SwaggerUIBundle.presets.apis], layout: 'BaseLayout' });
</script>
</body>
</html>
""";
}

public partial class Program { }
