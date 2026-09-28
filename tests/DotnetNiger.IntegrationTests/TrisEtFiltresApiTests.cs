using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie les tris et filtres ajoutés aux listes publiques et d'administration.
/// </summary>
/// <remarks>
/// Ces tests appellent les contrôleurs directement, comme <see cref="CycleDeVieApiTests"/>,
/// en semant les données dans le domaine puis en les publiant, pour maîtriser les dates sans
/// dépendre du courrier électronique.
/// </remarks>
public sealed class TrisEtFiltresApiTests : IDisposable
{
    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly Guid _membreId;
    private readonly DateTime _maintenant = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public TrisEtFiltresApiTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-tris-{Guid.NewGuid():N}.db");

        _membreId = Guid.NewGuid();

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
        services.AddScoped<ICurrentUserService>(_ => UtilisateurFictif.Membre(_membreId));
        services.AddDotnetNigerPersistence(configuration);

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        var contexte = _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        contexte.Database.EnsureCreated();

        contexte.Users.Add(new ApplicationUser("membre@example.test")
        {
            Id = _membreId,
            UserName = "membre@example.test",
            NormalizedUserName = "MEMBRE@EXAMPLE.TEST",
            EmailConfirmed = true,
        });
        contexte.SaveChanges();
    }

    [Fact]
    public async Task TrierLesArticlesParTitre_PrendLePasSurLaPublication()
    {
        var ancien = new Article(_membreId, "Zéphyr", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        ancien.Publish(_maintenant);
        var recent = new Article(_membreId, "Bêta", "Un chapô", "Un contenu", ArticleCategory.LocalProjects);
        recent.Publish(_maintenant.AddHours(1));

        UnitOfWork().Repository<Article>().Add(ancien);
        UnitOfWork().Repository<Article>().Add(recent);
        Sauver();

        var controleur = new ArticlesController(
            UnitOfWork(),
            UtilisateurFictif.Administrateur(Guid.NewGuid()),
            Lettre(),
            TimeProvider.System);

        var resultat = await controleur.Lister(trier: "titre", cancellationToken: CancellationToken.None);

        var titres = resultat.Value!.Items.Select(article => article.Title).ToArray();

        // Sans tri, le plus récemment publié passerait en premier : le tri alphabétique
        // doit remplacer cet ordre, pas s'y rajouter.
        Assert.Equal(["Bêta", "Zéphyr"], titres);
    }

    [Fact]
    public async Task RechercherLesArticles_TrouveDansLeTitre()
    {
        var cible = new Article(_membreId, "Migrations EF Core", "Un chapô", "Un contenu", ArticleCategory.DotNetNews);
        cible.Publish(_maintenant);
        var autre = new Article(_membreId, "Carnet de communauté", "Un chapô", "Un contenu", ArticleCategory.Interview);
        autre.Publish(_maintenant.AddHours(1));

        UnitOfWork().Repository<Article>().Add(cible);
        UnitOfWork().Repository<Article>().Add(autre);
        Sauver();

        var controleur = new ArticlesController(
            UnitOfWork(),
            UtilisateurFictif.Administrateur(Guid.NewGuid()),
            Lettre(),
            TimeProvider.System);

        var resultat = await controleur.Lister(recherche: "EF Core", cancellationToken: CancellationToken.None);

        var article = Assert.Single(resultat.Value!.Items);

        Assert.Equal(cible.Id, article.Id);
    }

    [Fact]
    public async Task TrierLesRessourcesParTitre_EstAlphabetique()
    {
        var zebre = CreerResource("Zèbre", ResourceCategory.Library);
        zebre.Publish(_maintenant);
        var alpha = CreerResource("Alpha", ResourceCategory.Tutorial);
        alpha.Publish(_maintenant.AddHours(1));

        UnitOfWork().Repository<Resource>().Add(zebre);
        UnitOfWork().Repository<Resource>().Add(alpha);
        Sauver();

        var controleur = new ResourcesController(
            UnitOfWork(),
            UtilisateurFictif.Membre(_membreId),
            Lettre(),
            TimeProvider.System);

        var resultat = await controleur.Lister(trier: "titre", cancellationToken: CancellationToken.None);

        var titres = resultat.Value!.Items.Select(ressource => ressource.Title).ToArray();

        Assert.Equal(["Alpha", "Zèbre"], titres);
    }

    [Fact]
    public async Task RechercherLesRessources_TrouveDansLeFournisseur()
    {
        var cible = CreerResource("Scripts", ResourceCategory.Tool, "Microsoft");
        cible.Publish(_maintenant);
        var autre = CreerResource("Cartes mémoire", ResourceCategory.Guide);
        autre.Publish(_maintenant.AddHours(1));

        UnitOfWork().Repository<Resource>().Add(cible);
        UnitOfWork().Repository<Resource>().Add(autre);
        Sauver();

        var controleur = new ResourcesController(
            UnitOfWork(),
            UtilisateurFictif.Membre(_membreId),
            Lettre(),
            TimeProvider.System);

        var resultat = await controleur.Lister(recherche: "Microsoft", cancellationToken: CancellationToken.None);

        var ressource = Assert.Single(resultat.Value!.Items);

        Assert.Equal(cible.Id, ressource.Id);
    }

    [Fact]
    public async Task FiltrerLesProjetsParTechnologie_NeGardeQueLesCorrespondances()
    {
        var aspnet = new Project(_membreId, "Site de la communauté", "Un résumé", "Une description", "https://github.com/exemple/site");
        aspnet.SetTechnologies("C#; ASP.NET Core");
        aspnet.Publish(_maintenant);
        var mobile = new Project(_membreId, "Appli mobile", "Un résumé", "Une description", "https://github.com/exemple/app");
        mobile.SetTechnologies("Flutter; Dart");
        mobile.Publish(_maintenant.AddHours(1));

        UnitOfWork().Repository<Project>().Add(aspnet);
        UnitOfWork().Repository<Project>().Add(mobile);
        Sauver();

        var controleur = new ProjectsController(
            UnitOfWork(),
            UtilisateurFictif.Membre(_membreId),
            Lettre(),
            TimeProvider.System);

        var resultat = await controleur.Lister(technologies: "ASP.NET", cancellationToken: CancellationToken.None);

        var projet = Assert.Single(resultat.Value!.Items);

        Assert.Equal(aspnet.Id, projet.Id);
    }

    [Fact]
    public async Task FiltrerLesEvenementsParFormat_NeGardeQueCeFormat()
    {
        var enLigne = new CommunityEvent(
            _membreId,
            "Atelier tuilage C#",
            "Un résumé",
            "Une description",
            CommunityEventFormat.Online,
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(10).AddHours(2));
        enLigne.SetOnlineUrl("https://meet.test/atelier");
        enLigne.Publish(_maintenant.AddDays(1));
        var presentiel = new CommunityEvent(
            _membreId,
            "Soirée .NET Niger",
            "Un résumé",
            "Une description",
            CommunityEventFormat.InPerson,
            DateTime.UtcNow.AddDays(20),
            DateTime.UtcNow.AddDays(20).AddHours(3));
        presentiel.SetLocation("Niamey");
        presentiel.Publish(_maintenant.AddDays(2));

        UnitOfWork().Repository<CommunityEvent>().Add(enLigne);
        UnitOfWork().Repository<CommunityEvent>().Add(presentiel);
        Sauver();

        var controleur = EvenementsControleur();

        var resultat = await controleur.Lister(
            format: CommunityEventFormat.Online,
            cancellationToken: CancellationToken.None);

        var evenement = Assert.Single(resultat.Value!.Items);

        Assert.Equal(enLigne.Id, evenement.Id);
        Assert.Equal(CommunityEventFormat.Online, evenement.Format);
    }

    [Fact]
    public async Task ListerLesEvenementsEnAdministration_ResoutLesBrouillons()
    {
        var brouillon = new CommunityEvent(
            _membreId,
            "Rencontre en préparation",
            "Un résumé",
            "Une description",
            CommunityEventFormat.Hybrid,
            _maintenant.AddDays(30),
            _maintenant.AddDays(30).AddHours(3));

        UnitOfWork().Repository<CommunityEvent>().Add(brouillon);
        Sauver();

        var administration = new AdminEventsController(
            UnitOfWork(),
            UtilisateurFictif.Administrateur(Guid.NewGuid()));

        var resultat = await administration.Lister(cancellationToken: CancellationToken.None);

        var evenement = Assert.Single(resultat.Value!.Items);

        Assert.Equal(brouillon.Id, evenement.Id);
        Assert.Equal(PublicationStatus.Draft, evenement.Status);
    }

    [Fact]
    public async Task ListerLesRessourcesEnAdministration_ParUnMembre_EstRefuse()
    {
        var controleur = new AdminResourcesController(UnitOfWork(), UtilisateurFictif.Membre(_membreId));

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.Lister(cancellationToken: CancellationToken.None));

        Assert.Equal("forbidden", erreur.Code);
    }

    [Fact]
    public async Task ListerLesProjetsEnAdministration_ParUnMembre_EstRefuse()
    {
        var controleur = new AdminProjectsController(UnitOfWork(), UtilisateurFictif.Membre(_membreId));

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.Lister(cancellationToken: CancellationToken.None));

        Assert.Equal("forbidden", erreur.Code);
    }

    [Fact]
    public async Task ListerLesEvenementsEnAdministration_ParUnMembre_EstRefuse()
    {
        var controleur = new AdminEventsController(UnitOfWork(), UtilisateurFictif.Membre(_membreId));

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.Lister(cancellationToken: CancellationToken.None));

        Assert.Equal("forbidden", erreur.Code);
    }

    private Resource CreerResource(string titre, ResourceCategory categorie, string? fournisseur = null)
    {
        var ressource = new Resource(_membreId, titre, "Une description", categorie);
        ressource.LinkTo("https://github.com/exemple", fournisseur ?? "DotnetNiger");

        return ressource;
    }

    private void Sauver() =>
        UnitOfWork().SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();

    private EventsController EvenementsControleur() =>
        new(
            UnitOfWork(),
            UtilisateurFictif.Membre(_membreId),
            _portee.ServiceProvider.GetRequiredService<IEmailDispatcher>(),
            _portee.ServiceProvider.GetRequiredService<IMemberDirectory>(),
            _portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            _portee.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>(),
            TimeProvider.System,
            NullLogger<EventsController>.Instance);

    private INewsletterDiffusion Lettre() =>
        _portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>();

    private IUnitOfWork UnitOfWork() => _portee.ServiceProvider.GetRequiredService<IUnitOfWork>();

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