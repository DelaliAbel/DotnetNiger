using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Monte un vrai <see cref="IServiceProvider"/> Identity au-dessus d'une base SQLite
/// temporaire.
/// </summary>
/// <remarks>
/// Le service de rattachement externe s'appuie sur <c>UserManager</c> : le tester avec un
/// double ne prouverait rien, puisque tout l'enjeu du code est de refuser un rattachement.
/// Le conteneur est donc construit comme dans l'application, sur une base jetable. Une seule
/// portée de services est ouverte pour toute la classe de tests, et fermée à la fin : en
/// ouvrir une par appel perdrait les modifications non enregistrées.
/// </remarks>
public sealed class IdentityFixture : IDisposable
{
    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;

    public IdentityFixture()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-identity-{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddHttpContextAccessor();

        // SignInManager ouvre et ferme des sessions : il lui faut les schémas
        // d'authentification, enregistrés par AddAuthentication, qu'un conteneur monté à la
        // main ne reçoit pas de l'hôte Web. L'accès au contexte HTTP, lui, passe par
        // AddHttpContextAccessor — mais sans requête en cours il ne renvoie rien : on installe
        // un contexte de test pour que la connexion par mot de passe fonctionne.
        services.AddAuthentication();
        services.AddAuthorization();

        // Les jetons d'Identity (confirmation, mot de passe oublié) reposent sur Data
        // Protection. L'application l'ajoute implicitement via le hôte Web ; un conteneur
        // monté à la main doit le faire explicitement, sinon la résolution du premier
        // UserManager échoue sur une dépendance absente.
        services.AddDataProtection();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICurrentUserService>(_ => UtilisateurFictif.Anonyme);

        services.AddDbContext<DotnetNigerDbContext>(options =>
            options.UseSqlite($"Data Source={_cheminBase}"));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<DotnetNigerDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        var contexte = _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        contexte.Database.EnsureCreated();

        // SignInManager écrit le jeton de session dans la réponse : sans requête, l'accès au
        // contexte échouerait sur « HttpContext must not be null ». Les tests de connexion
        // s'en servent pour vérifier qu'un mot de passe a réellement changé.
        _portee.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext();

        // Les rôles sont créés comme au démarrage de l'application : le rattachement d'un
        // compte externe en a besoin, et son absence masquerait la vraie cause d'un échec.
        var roles = _portee.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        roles.CreateAsync(new ApplicationRole("Member")).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gestionnaire de comptes branché sur la base de test.
    /// </summary>
    public UserManager<ApplicationUser> UserManager =>
        _portee.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    /// <summary>
    /// Construit un service de rattachement pour une requête donnée.
    /// </summary>
    /// <param name="httpContextAccessor">Accesseur de contexte de la requête.</param>
    /// <returns>Service de rattachement externe.</returns>
    public ExternalAccountService CreateAccountService(IHttpContextAccessor httpContextAccessor) =>
        new(
            UserManager,
            httpContextAccessor,
            _portee.ServiceProvider.GetRequiredService<ILogger<ExternalAccountService>>());

    /// <summary>
    /// Gestionnaire de connexion, pour les parcours qui ouvrent ou ferment une session.
    /// </summary>
    public SignInManager<ApplicationUser> SignInManager =>
        _portee.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

    /// <summary>
    /// Accesseur de contexte sans utilisateur connecté.
    /// </summary>
    /// <returns>Accesseur prêt à l'emploi.</returns>
    public static IHttpContextAccessor CreateAnonymousAccessor() => new HttpContextAccessor();

    public void Dispose()
    {
        _portee.Dispose();
        _fournisseur.Dispose();
        SqliteConnection.ClearAllPools();

        foreach (var fichier in new[] { _cheminBase, _cheminBase + "-shm", _cheminBase + "-wal" })
        {
            if (File.Exists(fichier))
            {
                File.Delete(fichier);
            }
        }
    }
}
