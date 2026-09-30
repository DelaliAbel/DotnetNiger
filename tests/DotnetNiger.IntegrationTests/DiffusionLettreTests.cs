using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie que la publication d'un événement, d'un article, d'une ressource ou d'un projet
/// met la lettre en file, et une seule fois.
/// </summary>
/// <remarks>
/// Ce que ces tests protègent tient en une phrase : la publication est le déclencheur de la
/// lettre, et elle ne déclenche qu'un envoi. Un doublon serait le pire défaut possible ici :
/// personne ne se désabonne pour avoir reçu la même lettre deux fois, et l'association
/// s'en apercevrait trop tard.
/// </remarks>
public sealed class DiffusionLettreTests : IDisposable
{
    private const string AdresseAwa = "awa.traore@exemple.test";
    private const string AdresseMoussa = "moussa.diallo@exemple.test";

    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly Guid _membreId;

    public DiffusionLettreTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-diffusion-{Guid.NewGuid():N}.db");

        // Le contenu publie porte l'identite de son auteur : sans cette ligne, la cle
        // etrangere refuserait l'insertion, et le test testerait la contrainte, pas la lettre.
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

        services.Configure<EmailOptions>(options => options.SiteUrl = "https://dotnetniger.test");

        services.AddScoped<ICurrentUserService>(
            _ => UtilisateurFictif.Administrateur(_membreId));

        services.AddDotnetNigerPersistence(configuration);

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        Contexte.Database.EnsureCreated();

        Contexte.Users.Add(new ApplicationUser("admin@exemple.test")
        {
            Id = _membreId,
            UserName = "admin@exemple.test",
            NormalizedUserName = "ADMIN@EXEMPLE.TEST",
            EmailConfirmed = true,
        });
        Contexte.SaveChanges();
    }

    [Fact]
    public async Task PublierUnArticleMetLaLettreEnFile()
    {
        await ConfirmerAsync(AdresseAwa);

        await PublierUnArticleAsync();

        var lettre = Assert.Single(await LettresEnFileAsync());
        Assert.Equal(AdresseAwa, lettre.Recipient);
        Assert.Contains("Article", lettre.Subject, StringComparison.Ordinal);
        Assert.Contains("Un article de démonstration", lettre.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LaLetreAnnonceLAdresseDuContenuPublie()
    {
        await ConfirmerAsync(AdresseAwa);

        var article = await PublierUnArticleAsync();

        var lettre = Assert.Single(await LettresEnFileAsync());

        // Le lien est construit à partir du slug, et non du titre : c'est lui qui reste
        // valable si le titre est corrigé après l'envoi.
        Assert.Contains($"/articles/{article.Slug}", lettre.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SeulesLesAdressesConfirmeesRecoiventLaLettre()
    {
        await ConfirmerAsync(AdresseAwa);
        await DemanderAsync(AdresseMoussa);

        await PublierUnArticleAsync();

        // Moussa a demandé l'abonnement sans le confirmer : une adresse qui n'a pas encore dit
        // oui ne reçoit rien.
        Assert.Equal(AdresseAwa, Assert.Single(await LettresEnFileAsync()).Recipient);
    }

    [Fact]
    public async Task DeuxAbonnesConfirmesRecoiventChacunLeurMessage()
    {
        await ConfirmerAsync(AdresseAwa);
        await ConfirmerAsync(AdresseMoussa);

        await PublierUnArticleAsync();

        // Deux messages distincts, jamais une liste en copie cachée : c'est la condition du
        // désabonnement individuel.
        Assert.Equal(
            [AdresseAwa, AdresseMoussa],
            (await LettresEnFileAsync()).Select(l => l.Recipient).Order().ToArray());
    }

    [Fact]
    public async Task ChaqueLettreContientSonLienDeDesabonnement()
    {
        await ConfirmerAsync(AdresseAwa);
        await ConfirmerAsync(AdresseMoussa);

        await PublierUnArticleAsync();

        var lettres = await LettresEnFileAsync();
        var cles = await AbonnementsAsync();

        // Un lien partagé ferait se désabonner tout le monde dès que l'un d'eux s'en va.
        foreach (var lettre in lettres)
        {
            var abonne = Assert.Single(cles, a => a.Email == lettre.Recipient);
            Assert.Contains(abonne.UnsubscribeKey, lettre.TextBody, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task RepublicerUnArticleNeRedonnePasLaLettre()
    {
        await ConfirmerAsync(AdresseAwa);

        var article = await PublierUnArticleAsync();
        await PublierArticleAsync(article.Id);
        await PublierArticleAsync(article.Id);

        // Une seule lettre par publication : trois corrections successives du même article
        // ne doivent pas produire trois courriels à tous les abonnés.
        Assert.Single(await LettresEnFileAsync());
    }

    [Fact]
    public async Task UnAbonneDesabonneNeRecoitPlusLaLettre()
    {
        await ConfirmerAsync(AdresseAwa);
        await ConfirmerAsync(AdresseMoussa);
        await DesabonnerAsync(AdresseAwa);

        await PublierUnArticleAsync();

        Assert.Equal(AdresseMoussa, Assert.Single(await LettresEnFileAsync()).Recipient);
    }

    [Fact]
    public async Task UnePublicationSansAbonneNEnvoyeRien()
    {
        await PublierUnArticleAsync();

        // Aucun abonné ne signifie aucun message, et surtout aucune erreur : publier un
        // article doit rester possible sur une lettre encore vide.
        Assert.Empty(await LettresEnFileAsync());
    }

    [Fact]
    public async Task PublierUnEvenementAnnonceLEvenement()
    {
        await ConfirmerAsync(AdresseAwa);

        await PublierUnEvenementAsync();

        var lettre = Assert.Single(await LettresEnFileAsync());
        Assert.Contains("Événement", lettre.Subject, StringComparison.Ordinal);
        Assert.Contains("/evenements/", lettre.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublierUneRessourceEnvoieLeTexteDeSaDescription()
    {
        await ConfirmerAsync(AdresseAwa);

        await PublierUneRessourceAsync();

        var lettre = Assert.Single(await LettresEnFileAsync());

        // La description d'une ressource est rédigée en HTML, pour les pages du site. La
        // lettre doit donc la réduire à du texte : une balise y apparaîtrait mot pour mot.
        Assert.Contains("Un guide pas a pas", lettre.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<p>", lettre.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublierUnProjetAnnonceLeProjet()
    {
        await ConfirmerAsync(AdresseAwa);

        await PublierUnProjetAsync();

        var lettre = Assert.Single(await LettresEnFileAsync());
        Assert.Contains("Projet", lettre.Subject, StringComparison.Ordinal);
        Assert.Contains("/projets/", lettre.HtmlBody, StringComparison.Ordinal);
    }

    private async Task<Article> PublierUnArticleAsync()
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new ArticlesController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(_membreId),
            portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            TimeProvider.System);

        var resultat = await controleur.Creer(
            new CreateArticleRequest(
                "Un article de démonstration",
                "Un chapô de démonstration",
                "Le contenu de l'article.",
                RubriqueDeTest.Id(ContexteTest),
                Publish: true),
            CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(resultat.Result);

        return ChargerDernierArticle();
    }

    private async Task PublierArticleAsync(Guid id)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new ArticlesController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(_membreId),
            portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            TimeProvider.System);

        await controleur.Publier(id, CancellationToken.None);
    }

    private async Task PublierUnEvenementAsync()
    {
        using var portee = _fournisseur.CreateScope();
        var debut = DateTime.UtcNow.AddDays(7);

        var controleur = new EventsController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(_membreId),
            portee.ServiceProvider.GetRequiredService<IEmailDispatcher>(),
            portee.ServiceProvider.GetRequiredService<IMemberDirectory>(),
            portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            portee.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>(),
            TimeProvider.System,
            NullLogger<EventsController>.Instance);

        await controleur.Creer(
            new CreateEventRequest(
                "Une soirée .NET",
                "Un résumé de soirée",
                "Une description de soirée.",
                CommunityEventFormat.InPerson,
                debut,
                debut.AddHours(3),
                "Niamey",
                Publish: true),
            CancellationToken.None);
    }

    private async Task PublierUneRessourceAsync()
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new ResourcesController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(_membreId),
            portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            TimeProvider.System);

        await controleur.Creer(
            new CreateResourceRequest(
                "Un guide pas a pas",
                "<p>Un guide pas a pas pour debutant.</p>",
                ResourceCategory.Guide),
            CancellationToken.None);
    }

    private async Task PublierUnProjetAsync()
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new ProjectsController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(_membreId),
            portee.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            TimeProvider.System);

        var creation = await controleur.Creer(
            new CreateProjectRequest(
                "Un projet de demonstration",
                "Un resume de projet",
                "Une description de projet.",
                "https://exemple.test/depot",
                "C#"),
            CancellationToken.None);

        var identifiant = Assert.IsType<ProjectSummaryResponse>(
            Assert.IsType<CreatedAtActionResult>(creation.Result).Value).Id;

        // Une portee neuve, comme le ferait une seconde requete : la creation et la
        // publication ne sont jamais demandees dans la meme.
        using var publication = _fournisseur.CreateScope();

        var controleurPublication = new ProjectsController(
            publication.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(_membreId),
            publication.ServiceProvider.GetRequiredService<INewsletterDiffusion>(),
            TimeProvider.System);

        await controleurPublication.Publier(identifiant, CancellationToken.None);
    }

    private Article ChargerDernierArticle() =>
        Contexte.Articles.AsNoTracking().OrderByDescending(a => a.CreatedAtUtc).First();

    private async Task DemanderAsync(string email)
    {
        var abonnement = NewsletterSubscription.Create(
            email,
            SubscriptionToken.Hasher("jeton"),
            SubscriptionToken.CreerCleDesabonnement(),
            DateTime.UtcNow);

        Contexte.NewsletterSubscriptions.Add(abonnement);
        await Contexte.SaveChangesAsync();
    }

    private async Task ConfirmerAsync(string email)
    {
        await DemanderAsync(email);

        var abonnement = await Contexte.NewsletterSubscriptions
            .SingleAsync(a => a.Email == email);

        abonnement.Confirm(SubscriptionToken.Hasher("jeton"), DateTime.UtcNow);
        await Contexte.SaveChangesAsync();
    }

    private async Task DesabonnerAsync(string email)
    {
        var abonnement = await Contexte.NewsletterSubscriptions.SingleAsync(a => a.Email == email);
        abonnement.Unsubscribe(abonnement.UnsubscribeKey, DateTime.UtcNow);
        await Contexte.SaveChangesAsync();
    }

    private async Task<List<NewsletterSubscription>> AbonnementsAsync() =>
        await Contexte.NewsletterSubscriptions.AsNoTracking().ToListAsync();

    /// <summary>
    /// Ne garde que les envois de la lettre : la file contient aussi les confirmations
    /// d'abonnement, qui ne disent rien de la diffusion.
    /// </summary>
    private async Task<List<OutboxMessage>> LettresEnFileAsync() =>
        [.. (await Contexte.OutboxMessages.AsNoTracking().ToListAsync())
            .Where(message => message.Kind == EmailTemplateKeys.NewsletterNewContent)];

    private DotnetNigerDbContext Contexte =>
        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

    /// <summary>
    /// Contexte du test : celui qui possède les lignes créées ici.
    /// </summary>
    /// <remarks>
    /// Une seule portée pour toute la classe. La rubrique de référence doit venir du
    /// contexte qui va posséder l'article, sinon EF lareattache comme nouvelle et
    /// échoue sur l'unicité du slug.
    /// </remarks>
    private DotnetNigerDbContext ContexteTest
        => _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

    public void Dispose()
    {
        _portee.Dispose();
        _fournisseur.Dispose();

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (File.Exists(_cheminBase))
        {
            File.Delete(_cheminBase);
        }
    }
}
