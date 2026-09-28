using DotnetNiger.Application.Common;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Infrastructure;

/// <summary>
/// Enregistrement de l'authentification ASP.NET Core Identity.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Déclare les comptes, les rôles, les cookies et les règles de mot de passe.
    /// </summary>
    /// <param name="services">Conteneur de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>Le même conteneur, pour permettre le chaînage.</returns>
    /// <remarks>
    /// Les règles de mot de passe, la durée de confirmation et le verrouillage sont lus
    /// dans la section <c>AccountSecurity</c>, afin de pouvoir les durcir en production
    /// sans recompiler.
    /// </remarks>
    public static IServiceCollection AddDotnetNigerIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var security = configuration
            .GetSection(AccountSecurityOptions.SectionName)
            .Get<AccountSecurityOptions>() ?? new AccountSecurityOptions();

        services.Configure<AccountSecurityOptions>(
            configuration.GetSection(AccountSecurityOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<AdminBootstrapOptions>(
            configuration.GetSection(AdminBootstrapOptions.SectionName));

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IExternalAccountService, ExternalAccountService>();
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequiredLength = security.MinimumPasswordLength;
                options.Password.RequireNonAlphanumeric = security.RequireNonAlphanumeric;
                options.Password.RequireDigit = security.RequireDigit;
                options.Password.RequireUppercase = security.RequireUppercase;
                options.Password.RequireLowercase = security.RequireLowercase;

                options.Lockout.MaxFailedAccessAttempts = security.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(security.LockoutMinutes);
                options.Lockout.AllowedForNewUsers = true;

            })
            .AddEntityFrameworkStores<DotnetNigerDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "dotnetniger.auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;

            // Le site étant sa propre interface, un composant Blazor ne peut pas suivre une
            // redirection HTTP vers la page de connexion : la demande de redirection arrive
            // au format d'un composant interactif. Le chemin est déclaré pour les requêtes
            // ordinaires, et les pages protégées gèrent l'état « non connecté » elles-mêmes.
            options.LoginPath = "/compte/connexion";
            options.LogoutPath = "/compte/deconnexion";
            options.AccessDeniedPath = "/acces-refuse";

            // Un client d'API n'affichera jamais une page de connexion : il attend un code
            // d'état, pas une redirection. Rediriger produirait un 302 suivi d'un document
            // HTML, que l'appelant devrait interpréter à la main. Les requêtes "/api"
            // reçoivent donc 401 et 403, comme le veut une API.
            options.Events.OnRedirectToLogin = contexte =>
            {
                if (EstAppelApi(contexte.Request))
                {
                    contexte.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    contexte.Response.Redirect(contexte.RedirectUri);
                }

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = contexte =>
            {
                if (EstAppelApi(contexte.Request))
                {
                    contexte.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
                else
                {
                    contexte.Response.Redirect(contexte.RedirectUri);
                }

                return Task.CompletedTask;
            };
        });

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            // Un cookie valide ne devrait pas être accepté plus d'un quart d'heure sans
            // revalidation, afin qu'une révocation de rôle prenne effet rapidement.
            options.ValidationInterval = TimeSpan.FromMinutes(15);
        });

        // Rôles et premier administrateur créés au démarrage, de façon idempotente.
        services.AddSingleton<IdentityBootstrapper>();
        // AddIdentity enregistre déjà UserManager, RoleManager et SignInManager. Rien
        // n'est redéclaré ici : une seconde inscription de RoleManager créerait deux
        // résolutions possibles pour le même service, ce que l'injection ne départage pas.
        return services;
    }

    /// <summary>
    /// Indique si la requête vise l'API plutôt que le site.
    /// </summary>
    /// <param name="requete">Requête entrante.</param>
    /// <returns><see langword="true"/> pour un chemin d'API.</returns>
    /// <remarks>
    /// Le préfixe est le même que celui porté par les contrôleurs, en
    /// <c>DotnetNiger.Web.Controller.Contracts.ApiRoutes</c>. Il est recopié ici car Infrastructure ne
    /// référence pas le projet Web, et la couche ne doit pas créer cette dépendance.
    /// </remarks>
    private static bool EstAppelApi(HttpRequest requete) =>
        requete.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}
