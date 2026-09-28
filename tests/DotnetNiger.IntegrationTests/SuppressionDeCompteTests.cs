using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie la création de comptes par l'administration et la suppression définitive d'un
/// compte avec ce qu'il a produit.
/// </summary>
/// <remarks>
/// Ces tests montent une vraie base SQLite et un vrai <c>UserManager</c>, parce que
/// l'enjeu est précisément le comportement du moteur : clés étrangères en
/// <c>Restrict</c>, cascades en base, et intercepteur qui transforme une suppression suivie
/// d'un <c>SaveChanges</c> en retrait logique. Un double ne prouverait rien de tout cela.
/// </remarks>
public sealed class SuppressionDeCompteTests : IDisposable
{
    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly Guid _membreId;
    private readonly Guid _autreId;

    public SuppressionDeCompteTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-suppression-{Guid.NewGuid():N}.db");

        _membreId = Guid.NewGuid();
        _autreId = Guid.NewGuid();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_cheminBase}",
                ["Database:Provider"] = "Sqlite",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();

        // La suppression du compte déconnecte la session : le SignInManager a besoin d'un
        // schéma d'authentification et d'une vraie réponse HTTP, sinon la déconnexion
        // échouerait avant même d'atteindre la suppression.
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme);

        // Identity résout ses fournisseurs de jetons dès la construction du UserManager ;
        // Data Protection est ajouté implicitement par l'hôte web, il doit donc l'être
        // explicitement dans un conteneur monté à la main.
        services.AddDataProtection();

        services.AddScoped<ICurrentUserService>(_ => UtilisateurFictif.Membre(_membreId));
        services.AddDotnetNigerPersistence(configuration);

        // Le contrôleur de compte se connecte au dernier administrateur actif : sans
        // UserManager, la règle anti-blocage ne pourrait pas être vérifiée.
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

        services.AddScoped<AccountDeletionService>();

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        var contexte = Contexte;
        contexte.Database.EnsureCreated();

        contexte.Users.Add(CreerUtilisateur(_membreId, "membre@example.test"));
        contexte.Users.Add(CreerUtilisateur(_autreId, "autre@example.test"));
        contexte.SaveChanges();

        var accessor = _portee.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext { RequestServices = _portee.ServiceProvider };

        var roles = _portee.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        roles.CreateAsync(new ApplicationRole("Member")).GetAwaiter().GetResult();
        roles.CreateAsync(new ApplicationRole("Admin")).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task SupprimerUnCompte_EmporteCeQuilAProduit()
    {
        var article = new Article(_membreId, "Un article épinglé", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        var ressource = new Resource(_membreId, "Un guide", "Un résumé", ResourceCategory.Guide);
        var projet = new Project(
            _membreId,
            "Un projet",
            "Un résumé",
            "Un contenu",
            "https://exemple.test/depot");
        var evenement = new CommunityEvent(
            _membreId,
            "Une rencontre",
            "Un résumé",
            "Une description",
            CommunityEventFormat.InPerson,
            new DateTime(2030, 1, 1, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 1, 20, 0, 0, DateTimeKind.Utc));
        var commentaire = Comment.ForArticle(article.Id, _membreId, "Un commentaire");
        var inscription = EventRegistration.Create(
            evenement.Id,
            _membreId,
            new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        Contexte.Articles.Add(article);
        Contexte.Resources.Add(ressource);
        Contexte.Projects.Add(projet);
        Contexte.CommunityEvents.Add(evenement);
        Contexte.Comments.Add(commentaire);
        Contexte.EventRegistrations.Add(inscription);
        await Contexte.SaveChangesAsync();

        var bilan = await ServiceDeSuppression().DeleteAsync(_membreId);

        Assert.NotNull(bilan);
        Assert.Equal(1, bilan.Articles);
        Assert.Equal(1, bilan.Resources);
        Assert.Equal(1, bilan.Projects);
        Assert.Equal(1, bilan.Events);
        Assert.Equal(1, bilan.Comments);
        Assert.Equal(1, bilan.Registrations);

        Assert.Empty(await Contexte.Articles.IgnoreQueryFilters().Where(a => a.Id == article.Id).ToListAsync());
        Assert.Empty(await Contexte.Comments.IgnoreQueryFilters().Where(c => c.Id == commentaire.Id).ToListAsync());
        Assert.Empty(await Contexte.Users.Where(u => u.Id == _membreId).ToListAsync());
    }

    [Fact]
    public async Task SupprimerUnCompte_LaisseIntactsLesContenusDesAutres()
    {
        var article = new Article(_autreId, "Un article partagé", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        var commentaire = Comment.ForArticle(article.Id, _autreId, "Un commentaire qui n'est pas de moi");

        Contexte.Articles.Add(article);
        Contexte.Comments.Add(commentaire);
        await Contexte.SaveChangesAsync();

        await ServiceDeSuppression().DeleteAsync(_membreId);

        // Le contenu d'autrui n'est pas la propriété du compte supprimé.
        Assert.Single(await Contexte.Articles.IgnoreQueryFilters().Where(a => a.Id == article.Id).ToListAsync());
        Assert.Single(await Contexte.Comments.IgnoreQueryFilters().Where(c => c.Id == commentaire.Id).ToListAsync());
        Assert.Single(await Contexte.Users.Where(u => u.Id == _autreId).ToListAsync());
    }

    [Fact]
    public async Task SupprimerUnCompte_RetireSesContenusDejaRetiresLogiquement()
    {
        var article = new Article(_membreId, "Un article retiré", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        Contexte.Articles.Add(article);
        await Contexte.SaveChangesAsync();

        article.SoftDelete(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        // Un article déjà retiré conserve sa clé étrangère vers son auteur : sans retirer
        // aussi les lignes filtrées, la suppression du compte échouerait sur la contrainte.
        await ServiceDeSuppression().DeleteAsync(_membreId);

        Assert.Empty(await Contexte.Articles.IgnoreQueryFilters().Where(a => a.Id == article.Id).ToListAsync());
    }

    [Fact]
    public async Task SupprimerUnCompte_RendALaModerationLesSignalementsTraites()
    {
        var cible = new Article(_autreId, "Un article signalé", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        Contexte.Articles.Add(cible);
        await Contexte.SaveChangesAsync();

        var signale = new Report(ReportTargetType.Article, cible.Id, ReportReason.Spam, _membreId);
        var traite = new Report(ReportTargetType.Article, cible.Id, ReportReason.Other, _autreId);

        Contexte.Reports.AddRange(signale, traite);
        await Contexte.SaveChangesAsync();

        signale.StartReview(_membreId);
        await Contexte.SaveChangesAsync();

        var bilan = await ServiceDeSuppression().DeleteAsync(_membreId);

        Assert.NotNull(bilan);
        Assert.Equal(1, bilan.Reports);
        Assert.Equal(1, bilan.ReleasedReports);

        // Le signalement déposé par le compte disparaît ; celui qu'il avait traité est
        // conservé, sans gestionnaire, pour que la décision de modération survive.
        Assert.Empty(await Contexte.Reports.Where(r => r.Id == signale.Id).ToListAsync());

        var restant = await Contexte.Reports.SingleAsync(r => r.Id == traite.Id);
        Assert.Equal(_autreId, restant.ReporterId);
        Assert.Null(restant.HandledById);
    }

    [Fact]
    public async Task SupprimerUnCompte_Inexistant_RenvoieNull()
    {
        Assert.Null(await ServiceDeSuppression().DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SupprimerSonCompte_SansConfirmation_EstRefuse()
    {
        var controleur = Controleur();

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.SupprimerSonCompte(
                new DeleteOwnAccountRequest("membre@example.test", ConfirmPermanentDeletion: false),
                CancellationToken.None));

        Assert.Equal("confirmation_required", erreur.Code);
        Assert.Single(await Contexte.Users.Where(u => u.Id == _membreId).ToListAsync());
    }

    [Fact]
    public async Task SupprimerSonCompte_AvecUneMauvaiseAdresse_EstRefuse()
    {
        var controleur = Controleur();

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.SupprimerSonCompte(
                new DeleteOwnAccountRequest("autre@example.test", ConfirmPermanentDeletion: true),
                CancellationToken.None));

        Assert.Equal("confirmation_mismatch", erreur.Code);
        Assert.Single(await Contexte.Users.Where(u => u.Id == _membreId).ToListAsync());
    }

    [Fact]
    public async Task SupprimerSonCompte_DoubleConfirmation_SupprimeLeCompte()
    {
        var article = new Article(_membreId, "Un article à emporter", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        Contexte.Articles.Add(article);
        await Contexte.SaveChangesAsync();

        var controleur = Controleur();

        var resultat = await controleur.SupprimerSonCompte(
            new DeleteOwnAccountRequest("MEMBRE@example.test", ConfirmPermanentDeletion: true),
            CancellationToken.None);

        Assert.IsType<NoContentResult>(resultat);
        Assert.Empty(await Contexte.Users.Where(u => u.Id == _membreId).ToListAsync());
        Assert.Empty(await Contexte.Articles.IgnoreQueryFilters().Where(a => a.Id == article.Id).ToListAsync());
    }

    [Fact]
    public async Task CreerUnCompte_DepuisLAdministration_LuiAttribueLeRoleMembre()
    {
        var controleur = new AdminAccountsController(
            UnitOfWork(),
            UtilisateurFictif.Administrateur(_autreId),
            Gestionnaire(),
            _portee.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>());

        var resultat = await controleur.Creer(
            new CreateAccountRequest(
                "nouveau@example.test",
                "MotDePasse!Solide2026",
                FirstName: "Awa",
                LastName: "Diallo",
                ConfirmEmail: true),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(resultat.Result);
        var compte = Assert.IsType<AdminAccountResponse>(created.Value);

        Assert.Equal("nouveau@example.test", compte.Email);
        Assert.True(compte.IsActive);
        Assert.True(compte.EmailConfirmed);
        Assert.Equal([RoleNames.Member], compte.Roles);

        var cree = await Gestionnaire().FindByIdAsync(compte.Id.ToString());
        Assert.NotNull(cree);
        Assert.Equal([RoleNames.Member], await Gestionnaire().GetRolesAsync(cree));
    }

    [Fact]
    public async Task CreerUnCompte_AvecUneAdresseDejaUtilisee_EstRefuse()
    {
        var controleur = new AdminAccountsController(
            UnitOfWork(),
            UtilisateurFictif.Administrateur(_autreId),
            Gestionnaire(),
            _portee.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>());

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.Creer(
                new CreateAccountRequest("membre@example.test", "MotDePasse!Solide2026"),
                CancellationToken.None));

        Assert.Equal("email_taken", erreur.Code);
    }

    private MembersController Controleur() => new(
        UnitOfWork(),
        UtilisateurFictif.Membre(_membreId),
        Gestionnaire(),
        _portee.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>(),
        ServiceDeSuppression());

    private AccountDeletionService ServiceDeSuppression() =>
        _portee.ServiceProvider.GetRequiredService<AccountDeletionService>();

    private UserManager<ApplicationUser> Gestionnaire() =>
        _portee.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    private IUnitOfWork UnitOfWork() =>
        _portee.ServiceProvider.GetRequiredService<IUnitOfWork>();

    private DotnetNigerDbContext Contexte =>
        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

    // Identity normalise l'adresse à la création : sans <c>NormalizedEmail</c>, une
    // recherche par adresse — le contrôle anti-doublon en dépend — ne trouverait rien.
    private static ApplicationUser CreerUtilisateur(Guid id, string email) => new(email)
    {
        Id = id,
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        NormalizedEmail = email.ToUpperInvariant(),
        EmailConfirmed = true,
    };

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
