using DotnetNiger.Api;
using DotnetNiger.Api.Options;
using DotnetNiger.Api.Seed;

// ============================================================
// BUILD
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// --- Configuration JWT ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException("JWT SecretKey must be configured and at least 32 characters long. Use user-secrets or environment variables.");

// --- Controllers ---
builder.Services.AddControllers()
    .AddApplicationPart(typeof(ServiceRegistration).Assembly)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddMemoryCache();

// --- Infrastructure ---
builder.Services.AddDatabaseWithIdentity(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddOAuthProviders(builder.Configuration);
builder.Services.ConfigureCookieAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCorsFromConfig(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddRateLimiting(builder.Configuration);

// --- Services métier ---
builder.Services.AddIdentityServices();

// ============================================================
// PIPELINE
// ============================================================

var app = builder.Build();

app.UsePipeline(builder.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{
    var adminPassword = builder.Configuration.GetValue<string>("AdminPassword")
        ?? throw new InvalidOperationException("AdminPassword must be configured in appsettings.json or environment variables.");
    await SeedData.InitializeAsync(app.Services, adminPassword);
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

var uploadsConfigured = app.Services.GetRequiredService<IOptions<UploadOptions>>().Value.Path;
var uploadsRoot = Path.GetFullPath(
    !string.IsNullOrWhiteSpace(uploadsConfigured)
        ? Path.Combine(app.Environment.ContentRootPath, uploadsConfigured)
        : Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads"));
app.MapGet("/uploads/{**path}", (string path) =>
{
    var filePath = Path.GetFullPath(Path.Combine(uploadsRoot, path));
    if (!filePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
        return Results.NotFound();
    return Results.File(filePath);
});



await app.RunAsync();
