// Composant Community: Program
using System.IO.Compression;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DotnetNiger.Community.Api.Extensions;
using DotnetNiger.Community.Api.Middleware;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Data;
using DotnetNiger.Community.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
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

// Configuration principale du service Community.
var connectionString = builder.Configuration.GetConnectionString("DotnetNigerDb")
    ?? throw new InvalidOperationException("Connection string 'DotnetNigerDb' introuvable.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DotnetNiger.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DotnetNiger.Identity.Client";
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("__") || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "La cle JWT n'est pas configuree. Definissez Jwt:Key (minimum 32 caracteres).");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddDbContext<CommunityDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddCommunityRepositories();
builder.Services.AddCommunityApplicationServices();

builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var identityBaseUrl = configuration["IdentityApi:BaseUrl"] ?? "http://localhost:5075/";
    client.BaseAddress = new Uri(identityBaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    ConnectTimeout = TimeSpan.FromSeconds(2),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    MaxConnectionsPerServer = 20
});

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
    options.AddPolicy("HotReadPolicy", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .SetVaryByQuery(new[] { "page", "pageSize", "search", "sortBy", "sortDirection" }));

    options.AddPolicy("StatsPolicy", policy =>
        policy.Expire(TimeSpan.FromSeconds(15)));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrSuperAdmin", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayOnly", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

await EnsureCommunityDataAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Community Service {description.GroupName}");
        }

        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Community Service - API Documentation";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.EnableDeepLinking();
        options.EnableFilter();
    });
}

app.UseHttpsRedirection();
app.UseCors("GatewayOnly");

app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseAuthentication();
app.UseCommunityMiddleware();
app.UseAuthorization();
app.UseOutputCache();

app.MapGet("/metrics/latency", () => Results.Ok(EndpointLatencyMetricsMiddleware.GetSnapshot()))
    .AllowAnonymous();

app.MapControllers();
app.Run();

// Migrations et seed au demarrage.
static async Task EnsureCommunityDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CommunityDbContext>();
    dbContext.Database.Migrate();
    await DatabaseSeeder.SeedDataAsync(dbContext);
}
