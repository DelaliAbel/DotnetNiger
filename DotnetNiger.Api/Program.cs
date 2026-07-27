using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using DotnetNiger.Api;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Data;
using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Seed;
using DotnetNiger.Api.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ============================================================
// BUILD
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// --- Configuration JWT ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException("JWT SecretKey must be configured and at least 32 characters long. Use user-secrets or environment variables.");

// --- Controllers + Swagger ---
builder.Services.AddControllers()
    .AddApplicationPart(typeof(DependencyInjection).Assembly)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez votre token JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme
          { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
          Array.Empty<string>() }
    });
});

builder.Services.AddMemoryCache();

// --- Base de données ---
builder.Services.AddDbContext<DotnetNigerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// --- ASP.NET Core Identity (Rôles natifs Microsoft) ---
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
})
.AddEntityFrameworkStores<DotnetNigerDbContext>()
.AddDefaultTokenProviders();

// --- SMTP ---
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

// --- Authentification JWT Bearer (remplace OpenIddict Validation) ---
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
});

// --- Google / GitHub / Microsoft OAuth ---
var googleSection = builder.Configuration.GetSection("Authentication:Google");
if (!string.IsNullOrEmpty(googleSection["ClientId"]) && googleSection["ClientId"] != "__SET_VIA_USER_SECRETS__")
{
    builder.Services.AddAuthentication().AddGoogle("Google", options =>
    {
        options.ClientId = googleSection["ClientId"]!;
        options.ClientSecret = googleSection["ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });
}

var githubSection = builder.Configuration.GetSection("Authentication:GitHub");
if (!string.IsNullOrEmpty(githubSection["ClientId"]) && githubSection["ClientId"] != "__SET_VIA_USER_SECRETS__")
{
    builder.Services.AddAuthentication().AddOAuth("GitHub", options =>
    {
        options.ClientId = githubSection["ClientId"]!;
        options.ClientSecret = githubSection["ClientSecret"]!;
        options.CallbackPath = "/signin-github";
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.Scope.Add("user:email");

        options.Events.OnCreatingTicket = async context =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
            using var response = await context.Backchannel.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (user.RootElement.TryGetProperty("id", out var id))
                context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.ToString() ?? ""));
            if (user.RootElement.TryGetProperty("login", out var login))
                context.Identity?.AddClaim(new Claim(ClaimTypes.Name, login.ToString() ?? ""));
            if (user.RootElement.TryGetProperty("email", out var email) && !email.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
                context.Identity?.AddClaim(new Claim(ClaimTypes.Email, email.ToString() ?? ""));
        };
    });
}

var microsoftSection = builder.Configuration.GetSection("Authentication:Microsoft");
if (!string.IsNullOrEmpty(microsoftSection["ClientId"]) && microsoftSection["ClientId"] != "__SET_VIA_USER_SECRETS__")
{
    builder.Services.AddAuthentication().AddMicrosoftAccount("Microsoft", options =>
    {
        options.ClientId = microsoftSection["ClientId"]!;
        options.ClientSecret = microsoftSection["ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.Scope.Add("https://graph.microsoft.com/User.Read");
    });
}

// --- Cookie auth pour le scheme externe (Google/GitHub) ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { error = "Non authentifié" });
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { error = "Accès refusé" });
    };
});

// --- Authorization (Rôles natifs + Permissions custom) ---
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Value;
    var origins = !string.IsNullOrWhiteSpace(allowedOrigins)
        ? allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        : [];

    options.AddDefaultPolicy(policy =>
    {
        if (origins.Length != 0)
            policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        else if (builder.Environment.IsDevelopment())
            policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader();
        else
            policy.SetIsOriginAllowed(_ => false);
    });
});

// --- Rate Limiting ---
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
var rateLimitOptions = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()!;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? (int)retryAfterValue.TotalSeconds
            : rateLimitOptions.WindowSeconds;
        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
        return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Trop de requêtes. Veuillez réessayer plus tard.",
            retryAfterSeconds = retryAfter
        }, cancellationToken));
    };

    options.AddPolicy("default", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetTokenBucketLimiter(
            $"default:{ip}",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = rateLimitOptions.PermitLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                TokensPerPeriod = rateLimitOptions.PermitLimit,
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.AddPolicy("Auth", httpContext =>
    {
        var clientId = httpContext.Request.Headers["ClientId"].FirstOrDefault() ?? "unknown";
        return RateLimitPartition.GetTokenBucketLimiter(
            $"auth:{clientId}",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = rateLimitOptions.AuthPermitLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.AuthWindowSeconds),
                TokensPerPeriod = rateLimitOptions.AuthPermitLimit,
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// --- Services métier ---
builder.Services.AddIdentityServices();

// ============================================================
// PIPELINE
// ============================================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    await SeedData.InitializeAsync(app.Services);
}

// Middleware d'erreur JSON pour les codes HTTP non gérés
app.Use(async (context, next) =>
{
    await next();
    if (!context.Response.HasStarted && context.Response.StatusCode is 404 or 401 or 403 or 429 or 500
        && context.Response.ContentType == null)
    {
        context.Response.ContentType = "application/json";
        var message = context.Response.StatusCode switch
        {
            401 => "Non authentifié",
            403 => "Accès refusé",
            404 => "Ressource introuvable",
            429 => "Trop de requêtes. Veuillez réessayer plus tard.",
            500 => "Erreur interne du serveur",
            _ => "Erreur"
        };
        await context.Response.WriteAsJsonAsync(new { error = message, statusCode = context.Response.StatusCode });
    }
});

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

await app.RunAsync();
