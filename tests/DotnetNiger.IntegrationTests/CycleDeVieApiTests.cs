using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie les gardes d'autorisation et les cycles de vie ajoutés après le premier
/// lot d'écritures.
/// </summary>
/// <remarks>
/// Ces tests visent la logique qui peut trancher à tort : savoir qui a le droit d'agir, et
/// savoir qu'une action a réellement changé l'état. Ils appellent les contrôleurs
/// directement, comme <see cref="EcritureApiTests"/>, pour rester rapides et sans hôte HTTP.
/// </remarks>
public sealed class CycleDeVieApiTests : IDisposable
{
    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly Guid _membreId;

    public CycleDeVieApiTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-cycle-{Guid.NewGuid():N}.db");

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
    public async Task ArchiverUnArticle_ChangeSonStatut()
    {
        var article = CreerArticle("Un article à archiver");
        var controleur = new ArticlesController(UnitOfWork(), UtilisateurFictif.Membre(_membreId), TimeProvider.System);

        await controleur.Archiver(article.Id, CancellationToken.None);

        var recharge = await Charger<Article>(article.Id);
        Assert.Equal(PublicationStatus.Archived, recharge.Status);
    }

    [Fact]
    public async Task MettreAJourUnArticle_ParUnAutreMembre_EstRefuse()
    {
        var article = CreerArticle("Un article protégé");
        var autreMembre = Guid.NewGuid();

        var controleur = new ArticlesController(UnitOfWork(), UtilisateurFictif.Membre(autreMembre), TimeProvider.System);

        var request = new UpdateArticleRequest(
            "Un titre usurpé",
            "Un chapô usurpé",
            "Un contenu usurpé",
            ArticleCategory.DotNetNews,
            "https://exemple.test/couverture.png");

        // Un membre qui n'est pas l'auteur ne doit pas pouvoir réécrire l'article.
        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.MettreAJour(article.Id, request, CancellationToken.None));

        Assert.Equal("forbidden", erreur.Code);
    }

    [Fact]
    public async Task MettreAJourUnArticle_ParUnAdministrateur_EstAutorise()
    {
        var article = CreerArticle("Un article de l'association");
        var administrateur = Guid.NewGuid();

        var controleur = new ArticlesController(UnitOfWork(), UtilisateurFictif.Administrateur(administrateur), TimeProvider.System);

        var request = new UpdateArticleRequest(
            "Un titre corrigé",
            "Un chapô corrigé",
            "Un contenu corrigé",
            ArticleCategory.DotNetNews,
            "https://exemple.test/nouvelle.png");

        var resultat = await controleur.MettreAJour(article.Id, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultat.Result);
        Assert.IsType<ArticleResponse>(ok.Value);

        var recharge = await Charger<Article>(article.Id);
        Assert.Equal("Un titre corrigé", recharge.Title);
        Assert.Equal("https://exemple.test/nouvelle.png", recharge.CoverImageUrl);
    }

    [Fact]
    public async Task SupprimerUnArticle_LeRetireSansEffacerLaLigne()
    {
        var article = CreerArticle("Un article à retirer");
        var controleur = new ArticlesController(UnitOfWork(), UtilisateurFictif.Membre(_membreId), TimeProvider.System);

        await controleur.Supprimer(article.Id, CancellationToken.None);

        // La suppression est logique : la ligne reste, avec sa date de retrait.
        var recharge = await Charger<Article>(article.Id);
        Assert.NotNull(recharge.DeletedAtUtc);
    }

    [Fact]
    public async Task CreerUnEvenement_ParUnMembre_EstRefuse()
    {
        var controleur = new EventsController(UnitOfWork(), UtilisateurFictif.Membre(_membreId), TimeProvider.System);

        var request = new CreateEventRequest(
            "Un événement de démonstration",
            "Un résumé",
            "Une description",
            CommunityEventFormat.InPerson,
            StartsAtUtc: new DateTime(2030, 1, 1, 18, 0, 0, DateTimeKind.Utc),
            EndsAtUtc: new DateTime(2030, 1, 1, 20, 0, 0, DateTimeKind.Utc));

        // Un événement est une initiative de l'association : seul un administrateur en crée.
        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.Creer(request, CancellationToken.None));

        Assert.Equal("forbidden", erreur.Code);
    }

    private Article CreerArticle(string titre)
    {
        var article = new Article(_membreId, titre, "Un chapô", "Un contenu", ArticleCategory.DotNetNews);

        UnitOfWork().Repository<Article>().Add(article);
        UnitOfWork().SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();

        // Le contrôleur partage ce même contexte : sans libérer le suivi, la ligne relue
        // par sa requête entrerait en conflit avec l'instance déjà suivie ici.
        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>().ChangeTracker.Clear();

        return article;
    }

    /// <summary>
    /// Relit une entite sans filtre global, afin de voir les lignes retirees.
    /// </summary>
    private async Task<T> Charger<T>(Guid id) where T : class
    {
        var contexte = _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

        return await contexte.Set<T>()
            .IgnoreQueryFilters()
            .SingleAsync(entite => EF.Property<Guid>(entite, "Id").Equals(id));
    }

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
