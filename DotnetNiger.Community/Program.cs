using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetNiger.Community.Api;
using DotnetNiger.Community.Api.Middleware;
using DotnetNiger.Community.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

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
builder.Services.AddCommunityAuthentication(builder.Configuration);
builder.Services.AddCommunityServices();
builder.Services.AddCommunityHttpClients(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DotnetNiger Community API v1");
        options.RoutePrefix = "";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    await TryRegisterWithGatewayAsync(builder.Configuration, app.Services);

    Log.Information("DotnetNiger.Community démarré sur {Urls}", string.Join(", ", app.Urls));
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task TryRegisterWithGatewayAsync(IConfiguration configuration, IServiceProvider services)
{
    var gatewayUrl = configuration["Gateway:RegistrationUrl"];
    if (string.IsNullOrWhiteSpace(gatewayUrl))
    {
        Log.Information("Gateway registration skipped: Gateway:RegistrationUrl not configured");
        return;
    }

    var registrationKey = configuration["Gateway:RegistrationKey"];
    var baseUrl = configuration["Jwt:Authority"] ?? "http://localhost:5269";

    try
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        if (!string.IsNullOrWhiteSpace(registrationKey) && !registrationKey.StartsWith("__"))
            client.DefaultRequestHeaders.Add("X-Registration-Key", registrationKey);

        var payload = new
        {
            id = "community",
            url = baseUrl.TrimEnd('/'),
            name = "Community API",
            healthEndpoint = "/api/v1/test/health",
            swaggerEndpoint = "/swagger/v1/swagger.json",
            containerName = "community",
            port = 8082
        };

        var response = await client.PostAsJsonAsync(gatewayUrl, payload);
        if (response.IsSuccessStatusCode)
            Log.Information("Registered with Gateway at {Url}", gatewayUrl);
        else
            Log.Warning("Gateway registration returned {StatusCode}", (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to register with Gateway at {Url}", gatewayUrl);
    }
}
