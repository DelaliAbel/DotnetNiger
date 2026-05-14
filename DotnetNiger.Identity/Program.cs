using System.Net.Http.Json;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using DotnetNiger.Identity.Infrastructure;
using DotnetNiger.Identity.Api;
using DotnetNiger.Identity.Api.Middleware;

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

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddIdentityInfrastructure(builder.Configuration, builder.Environment);
    builder.Services.AddIdentityServices();
    builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformer>();
    builder.Services.AddApiVersioningWithSwagger();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors("GatewayOnly");

    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<TenantResolutionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DotnetNiger Identity v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.MapGet("/health", () => Results.Ok(new
    {
        status = "Healthy",
        service = "DotnetNiger.Identity",
        timestamp = DateTime.UtcNow
    }));

    app.MapControllers();

    // Initialisation de la base de données au démarrage
    using (var scope = app.Services.CreateScope())
    {
        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.UserManager<DotnetNiger.Identity.Domain.Entities.ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.RoleManager<DotnetNiger.Identity.Domain.Entities.ApplicationRole>>();

        var adminPassword = builder.Configuration["Admin:DefaultPassword"] ?? "Admin@123456";
        await DbSeeder.SeedAsync(db, userManager, roleManager, tenantContext, adminPassword);
    }

    await TryRegisterWithGatewayAsync(builder.Configuration, app.Services);

    Log.Information("DotnetNiger.Identity démarré sur {Urls}", string.Join(", ", app.Urls));
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
    var baseUrl = configuration["Smtp:AppBaseUrl"] ?? "http://localhost:5075";

    try
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        if (!string.IsNullOrWhiteSpace(registrationKey) && !registrationKey.StartsWith("__"))
            client.DefaultRequestHeaders.Add("X-Registration-Key", registrationKey);

        var payload = new
        {
            id = "identity",
            url = baseUrl.TrimEnd('/'),
            name = "Identity API",
            healthEndpoint = "/api/v1/diagnostics/health",
            swaggerEndpoint = "/swagger/v1/swagger.json",
            containerName = "identity",
            port = 8081
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
