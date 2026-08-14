using DotnetNiger.Api;
using DotnetNiger.Api.Options;
using DotnetNiger.Api.Seed;

// ============================================================
// BUILD
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// --- Configuration JWT (validation au démarrage : config invalide = crash immédiat) ---
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();

// --- Controllers ---
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ServiceRegistration).Assembly)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Requis pour construire le middleware d'exceptions ; les IExceptionHandler
// écrivent eux-mêmes le corps JSON ({error, statusCode, detail}), ce service
// ne sert que de fallback quand aucun handler ne prend en charge l'exception.
builder.Services.AddProblemDetails();

builder.Services.AddMemoryCache();

// --- Infrastructure ---
builder.Services.AddDatabaseWithIdentity(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddOAuthProviders(builder.Configuration);
builder.Services.ConfigureCookieAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddExceptionHandlers();
builder.Services.AddCorsFromConfig(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddRateLimiting(builder.Configuration);

// --- Services métier ---
builder.Services.AddIdentityServices();

var seedEnabled = builder.Configuration.GetValue<bool>("Seed:Enabled", true);

// ============================================================
// PIPELINE
// ============================================================

var app = builder.Build();

app.UsePipeline(builder.Environment.IsDevelopment());

if (seedEnabled)
{
    var adminPassword = builder.Configuration.GetValue<string>("AdminPassword");
    if (string.IsNullOrWhiteSpace(adminPassword))
        throw new InvalidOperationException("AdminPassword must be configured in appsettings.Development.json, appsettings.Production.json or environment variables.");
    await SeedData.InitializeAsync(app.Services, adminPassword);
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapUploadsEndpoints();

await app.RunAsync();
