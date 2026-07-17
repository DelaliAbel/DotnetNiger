using Asp.Versioning;
using DotnetNiger.Common.Email;
using DotnetNiger.Community.Application.Notifications;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Api;

/// <summary>Méthodes d'extension pour enregistrer les services de la communauté dans le conteneur DI.</summary>
public static class ServiceExtensions
{
    /// <summary>Configure le DbContext avec le fournisseur de base de données (Sqlite, SqlServer ou PostgreSQL).</summary>
    public static IServiceCollection AddCommunityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required");
            options.UseSqlServer(connStr, x => x
                .MigrationsHistoryTable("__EFMigrationsHistory_Community")
                .MigrationsAssembly("DotnetNiger.Community")
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        services.AddHttpContextAccessor();
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailService, CommunityEmailSender>();

        return services;
    }

    /// <summary>Enregistre tous les services métier (posts, événements, ressources, etc.) en tant que services Scoped.</summary>
    public static IServiceCollection AddCommunityServices(this IServiceCollection services)
    {
        services.AddScoped<IPostQueryService, PostQueryService>();
        services.AddScoped<IPostCommandService, PostCommandService>();
        services.AddScoped<IPostModerationService, PostModerationService>();
        services.AddScoped<IEventQueryService, EventQueryService>();
        services.AddScoped<IEventCommandService, EventCommandService>();
        services.AddScoped<IEventModerationService, EventModerationService>();
        services.AddScoped<IEventRegistrationService, EventRegistrationService>();
        services.AddScoped<IResourceQueryService, ResourceQueryService>();
        services.AddScoped<IResourceCommandService, ResourceCommandService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IMemberDirectoryService, MemberDirectoryService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        services.AddMemoryCache();

        return services;
    }

    /// <summary>Configure l'authentification JWT avec Authority et les paramètres de validation.</summary>
    public static IServiceCollection AddCommunityAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Jwt:Authority"]!;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"]!,
                    ValidAudience = configuration["Jwt:Audience"] ?? "DotnetNiger.Identity.Client",
                };
                options.MetadataAddress = configuration["Jwt:MetadataAddress"]!;
                options.RequireHttpsMetadata = !environment.IsDevelopment() && !configuration.GetValue<bool>("Jwt:DisableHttpsRequirement");
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>Configure le client HTTP pour l'API d'identité (Identity API).</summary>
    public static IServiceCollection AddCommunityHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["Identity:ApiKey"] ?? configuration["Integration:ProvisioningApiKey"] ?? "";

        services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Identity:BaseUrl"] ?? "http://localhost:5075");
            if (!string.IsNullOrEmpty(apiKey))
                client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        });

        return services;
    }

    /// <summary>Configure CORS pour les origines autorisées (frontend Blazor WASM, etc.).</summary>
    public static IServiceCollection AddCommunityCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration["Cors:AllowedOrigins"] ?? "";
        var origins = allowedOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontendOrigins", policy =>
            {
                if (origins.Length > 0)
                    policy.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                else
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>Configure le versioning d'API (URL segment + rapport version).</summary>
    public static IServiceCollection AddApiVersioningWithSwagger(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
