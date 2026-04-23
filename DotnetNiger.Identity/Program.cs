// Composant Identity: Program
using System.IO.Compression;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DotnetNiger.Identity.Api.Middleware;
using DotnetNiger.Identity.Api.Extensions;
using DotnetNiger.Identity.Api.Filters;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.Data.Seeds;
using DotnetNiger.Identity.Infrastructure.External;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog avec sinks definis dans appsettings.
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Configuration principale du service Identity.
var connectionString = builder.Configuration.GetConnectionString("DotnetNigerDb") ?? throw new InvalidOperationException("Connection string 'DotnetNigerDb' introuvable.");

// Chargement de la configuration JWT.
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Section de configuration 'Jwt' introuvable.");

// Verification que la cle JWT a ete configuree (via variable d'environnement ou user-secrets).
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.StartsWith("__") || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException(
        "La cle JWT n'est pas configuree. Definissez la variable d'environnement Jwt__Key ou utilisez dotnet user-secrets. Minimum 32 caracteres.");
}

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Gestion centralisee des erreurs metier.
    options.Filters.Add<ExceptionFilter>();
    options.Filters.Add<ValidateModelFilter>();
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    // Format de groupe: v1, v1.0, etc.
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddDbContext<DotnetNigerIdentityDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<DotnetNigerIdentityDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection("FileUpload"));
builder.Services.Configure<AccountDeletionOptions>(builder.Configuration.GetSection("AccountDeletion"));
builder.Services.AddScoped<RefreshTokenGenerator>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ICommunityProvisioningClient, CommunityProvisioningClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var configuredBaseUrl = configuration["CommunityApi:BaseUrl"] ?? "http://localhost:5269/";
    if (!Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var communityBaseUri) ||
        (communityBaseUri.Scheme != Uri.UriSchemeHttp && communityBaseUri.Scheme != Uri.UriSchemeHttps))
    {
        throw new InvalidOperationException(
            $"CommunityApi:BaseUrl is invalid ('{configuredBaseUrl}'). Use an absolute HTTP/HTTPS URL.");
    }

    var timeoutSeconds = configuration.GetValue<int?>("CommunityApi:TimeoutSeconds") ?? 15;
    timeoutSeconds = Math.Clamp(timeoutSeconds, 3, 120);

    client.BaseAddress = communityBaseUri;
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    ConnectTimeout = TimeSpan.FromSeconds(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    MaxConnectionsPerServer = 20
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache(); // For rate limiting cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("IdentityReadPolicy", policy =>
        policy.Expire(TimeSpan.FromSeconds(20))
              .SetVaryByQuery(new[] { "page", "pageSize", "search", "sortBy", "sortDirection" }));
});

builder.Services.AddEmailProviders();
builder.Services.AddIdentityApplicationServices();
builder.Services.AddHostedService<AvatarCleanupService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Smart";
        options.DefaultChallengeScheme = "Smart";
    })
    .AddPolicyScheme("Smart", "Smart", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Headers.ContainsKey("X-API-Key")
                ? "ApiKey"
                : JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyOnly", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("scope", "api_key"));
});

//-----AjouterPourLaCommunicationExterne--------
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayOnly", policy =>
        policy.AllowAnyOrigin() // Remplace par l’URL réelle du Gateway si besoin [WithOrigins("http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod());
}
);
//-----------------------------------------------

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

var fileUploadOptions = app.Services.GetRequiredService<IOptions<FileUploadOptions>>().Value;
if (string.Equals(fileUploadOptions.Provider, "Local", StringComparison.OrdinalIgnoreCase))
{
    var uploadRoot = Path.Combine(app.Environment.ContentRootPath, fileUploadOptions.RootPath);
    Directory.CreateDirectory(uploadRoot);
    var publicBasePath = string.IsNullOrWhiteSpace(fileUploadOptions.PublicBasePath)
        ? "/uploads"
        : fileUploadOptions.PublicBasePath;
    if (!publicBasePath.StartsWith('/'))
    {
        publicBasePath = "/" + publicBasePath;
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadRoot),
        RequestPath = publicBasePath
    });
}

await EnsureIdentityCoreDataAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Identity Service {description.GroupName}");
        }

        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Identity Service - API Documentation";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.EnableDeepLinking();
        options.EnableFilter();
    });
}

app.UseHttpsRedirection();
app.UseCors("GatewayOnly");
// Journalisation structuree des requetes HTTP.
app.UseResponseCompression();
app.UseEndpointLatencyMetrics();
app.UseSerilogRequestLogging();
app.UseRequestLogging();
app.UseAuthentication();
app.UseJwtEnrichment();
app.UseAuthorization();
app.UseOutputCache();

app.MapGet("/metrics/latency", () => Results.Ok(EndpointLatencyMetricsMiddleware.GetSnapshot()))
    .AllowAnonymous();

app.MapControllers();
app.Run();

// Seeding au demarrage: roles + permissions de base.
static async Task EnsureIdentityCoreDataAsync(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
    await DefaultRolesSeeder.EnsureCoreDataAsync(app.Services, logger);
}
