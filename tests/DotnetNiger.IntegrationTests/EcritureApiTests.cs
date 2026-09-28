using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Infrastructure;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using DotnetNiger.Web.Controller;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie le chemin d'écriture de l'API avec un membre authentifié.
/// </summary>
/// <remarks>
/// Un test d'intégration HTTP complet serait plus proche de la réalité, mais il faudrait
/// monter l'hôte, piloter la confirmation d'adresse et l'envoi du courriel. Ce test vise
/// donc l'ensemble effectivement risqué après conversion en contrôleurs : l'identifiant de
/// l'auteur résolu par <see cref="ICurrentUserService"/>, l'écriture par l'unité de travail,
/// et la reprise du champ image. Les codes 401 et 400 sont, eux, vérifiés en SmokeTest.
/// </remarks>
public sealed class EcritureApiTests : IDisposable
{
    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly Guid _membreId;

    public EcritureApiTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-api-{Guid.NewGuid():N}.db");

        // L'identifiant est tiré avant le conteneur : l'intercepteur d'audit résout
        // ICurrentUserService dans le conteneur, et le contrôleur reçoit le même
        // utilisateur, ce qui rend l'écriture testée cohérente avec l'audit produit.
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

        // Le membre est inséré directement : ce test n'ouvre aucune session, il lui faut
        // seulement que la ligne existe pour que la résolution de l'auteur par le
        // contrôleur dispose d'un nom à renvoyer.
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
    public async Task CreerUnArticle_ParUnMembre_EstRefuse()
    {
        // Le blog est une publication de l'association : un membre ne rédige pas d'article.
        var controleur = ConstruireControleur(UtilisateurFictif.Membre(_membreId));
        var request = new CreateArticleRequest(
            "Un article de démonstration",
            "Un chapô court",
            "Le contenu rédigé de l'article.",
            ArticleCategory.DotNetNews,
            Publish: true,
            CoverImageUrl: "https://exemple.test/couverture.png");

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => controleur.Creer(request, CancellationToken.None));

        Assert.Equal("forbidden", erreur.Code);
    }

    [Fact]
    public async Task CreerUnArticle_LieLAuteurEtConserveLImage()
    {
        var controleur = ConstruireControleur(UtilisateurFictif.Administrateur(_membreId));
        var request = new CreateArticleRequest(
            "Un article de démonstration",
            "Un chapô court",
            "Le contenu rédigé de l'article.",
            ArticleCategory.DotNetNews,
            Publish: true,
            CoverImageUrl: "https://exemple.test/couverture.png");

        var resultat = await controleur.Creer(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(resultat.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);

        var slug = Assert.IsType<string>(created.RouteValues!["slug"]);

        var article = await _portee.ServiceProvider
            .GetRequiredService<DotnetNigerDbContext>()
            .Set<Article>()
            .SingleAsync(article => article.Slug == slug);

        // L'auteur doit être l'utilisateur connecté, jamais celui transmis par l'appelant.
        Assert.Equal(_membreId, article.AuthorId);
        Assert.Equal(PublicationStatus.Published, article.Status);
        Assert.Equal("https://exemple.test/couverture.png", article.CoverImageUrl);
    }

    [Fact]
    public async Task ListerLesArticles_RetourneUnePageVideSansErreur()
    {
        var controleur = ConstruireControleur(UtilisateurFictif.Anonyme);

        var resultat = await controleur.Lister(1, 20, null, CancellationToken.None);

        var page = Assert.IsType<PageResponse<ArticleSummaryResponse>>(resultat.Value);
        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
    }

    private ArticlesController ConstruireControleur(ICurrentUserService utilisateur) =>
        new(
            _portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            utilisateur,
            TimeProvider.System);

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
