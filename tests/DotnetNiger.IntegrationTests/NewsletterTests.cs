using System.Text.RegularExpressions;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie l'abonnement à la lettre d'information : double confirmation, désabonnement
/// individuel et diffusion.
/// </summary>
/// <remarks>
/// Ce que ces tests/protègent tient en une phrase : personne ne reçoit la lettre sans avoir
/// confirmé, et chacun peut se retirer sans que cela concerne les autres. Tout le reste en
/// découle — un lien deviné confirmerait une adresse qui n'a rien demandé, et un jeton
/// partagé vicierait tous les désabonnements à la fois.
/// </remarks>
public sealed partial class NewsletterTests : IDisposable
{
    private const string AdresseAwa = "awa.traore@exemple.test";
    private const string AdresseMoussa = "moussa.diallo@exemple.test";

    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;

    public NewsletterTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-lettre-{Guid.NewGuid():N}.db");

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

        services.Configure<EmailOptions>(options =>
        {
            options.SiteUrl = "https://dotnetniger.test";
            options.ContactAddress = "contact@dotnetniger.test";
        });

        services.AddScoped<ICurrentUserService>(
            _ => UtilisateurFictif.Administrateur(Guid.NewGuid()));

        services.AddDotnetNigerPersistence(configuration);

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        Contexte.Database.EnsureCreated();
    }

    [Fact]
    public async Task UnAbonnementEstAttenteDeConfirmation()
    {
        await SabonnerAsync(AdresseAwa);

        var abonnement = Assert.Single(await AbonnementsAsync());

        // Tant que l'adresse n'a pas confirmé, elle ne reçoit rien : c'est tout l'intérêt du
        // double consentement, et la raison pour laquelle aucun envoi ne part à ce stade.
        Assert.Equal(NewsletterStatus.Pending, abonnement.Status);
        Assert.Null(abonnement.ConfirmedAtUtc);
        Assert.Empty(await LettresEnFileAsync());
    }

    [Fact]
    public async Task UnCourrielDeConfirmationEstMisEnFile()
    {
        await SabonnerAsync(AdresseAwa);

        var message = Assert.Single(await MessagesEnFileAsync());

        Assert.Equal(EmailTemplateKeys.NewsletterConfirmation, message.Kind);
        Assert.Equal(AdresseAwa, message.Recipient);
    }

    [Fact]
    public async Task LeLienDeConfirmationContientUnJeton()
    {
        await SabonnerAsync(AdresseAwa);

        // Le lien doit porter un jeton, faute de quoi l'abonné n'a aucun moyen de confirmer.
        var lien = await JetonDeConfirmationAsync();

        Assert.False(string.IsNullOrWhiteSpace(lien));
    }

    [Fact]
    public async Task UneConfirmationValideActiveLabonnement()
    {
        await SabonnerAsync(AdresseAwa);
        var jeton = await JetonDeConfirmationAsync();

        await ConfirmerAsync(jeton);

        var abonnement = Assert.Single(await AbonnementsAsync());
        Assert.Equal(NewsletterStatus.Confirmed, abonnement.Status);
        Assert.NotNull(abonnement.ConfirmedAtUtc);
    }

    [Fact]
    public async Task UneConfirmationFalsifieeActiveRien()
    {
        await SabonnerAsync(AdresseAwa);

        await ConfirmerAsync("jeton-invente-1234");

        // Un jeton deviné ne confirme rien : c'est ce qui empêche d'inscrire quelqu'un à une
        // lettre qu'il n'a pas demandée.
        Assert.Equal(NewsletterStatus.Pending, Assert.Single(await AbonnementsAsync()).Status);
    }

    [Fact]
    public async Task UneConfirmationRepeteeNeChangeRien()
    {
        await SabonnerAsync(AdresseAwa);
        var jeton = await JetonDeConfirmationAsync();

        await ConfirmerAsync(jeton);
        var premiere = Assert.Single(await AbonnementsAsync()).ConfirmedAtUtc;
        await ConfirmerAsync(jeton);

        // La date d'origine ne doit pas bouger : elle dit depuis quand la personne reçoit.
        Assert.Equal(premiere, Assert.Single(await AbonnementsAsync()).ConfirmedAtUtc);
    }

    [Fact]
    public async Task DeuxAbonnementsALaMemeAdresseNEnFontQuUn()
    {
        await SabonnerAsync(AdresseAwa);
        await SabonnerAsync(AdresseAwa);

        // Une seule adresse ne peut avoir qu'un abonnement, sinon les lettres partiraient en
        // double pour certains et jamais pour d'autres selon quel lien a été cliqué.
        Assert.Single(await AbonnementsAsync());
    }

    [Fact]
    public async Task SeulesLesAdressesConfirmeesRecoiventLaLettre()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);
        await SabonnerAsync(AdresseMoussa);

        // Awa a confirmé, Moussa non. Seul Awa doit recevoir la lettre.
        var envoyes = await DiffuserAsync();

        Assert.Equal(1, envoyes.QueuedCount);

        var lettre = Assert.Single(await LettresEnFileAsync());
        Assert.Equal(AdresseAwa, lettre.Recipient);
    }

    [Fact]
    public async Task UneLettreALaAdresseCibleeContientSonLienDeDesabonnement()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);

        await DiffuserAsync();

        var lettre = Assert.Single(await LettresEnFileAsync());

        // Le lien de désabonnement est propre à Awa : un lien partagé ferait se désabonner
        // tout le monde dès que l'un d'eux s'en va.
        var cle = CleDesabonnement(await AbonnementAsync(AdresseAwa));
        Assert.Contains(cle, lettre.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeuxAbonnesConfirmesRecoiventChacunLeurMessage()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);
        await SabonnerEtConfirmerAsync(AdresseMoussa);

        var envoyes = await DiffuserAsync();

        // Deux messages distincts, jamais une liste en copie cachée : c'est la condition du
        // désabonnement individuel.
        Assert.Equal(2, envoyes.QueuedCount);

        var lettres = await LettresEnFileAsync();
        Assert.Equal(2, lettres.Count);
        Assert.Equal(
            [AdresseAwa, AdresseMoussa],
            lettres.Select(l => l.Recipient).Order().ToArray());
    }

    [Fact]
    public async Task UnDesabonnementRomptLinscription()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);
        var cle = CleDesabonnement(await AbonnementAsync(AdresseAwa));

        await DesabonnerAsync(cle);

        Assert.Equal(NewsletterStatus.Unsubscribed, (await AbonnementAsync(AdresseAwa)).Status);
    }

    [Fact]
    public async Task UnDesabonnementEstConfirmeParCourriel()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);

        await DesabonnerAsync(CleDesabonnement(await AbonnementAsync(AdresseAwa)));

        // La personne doit pouvoir vérifier qu'on a bien pris sa demande en compte, faute de
        // quoi elle recommencera etersection pensera que ça n'a pas marché.
        var confirmation = Assert.Single(
            await MessagesEnFileAsync(),
            message => message.Kind == EmailTemplateKeys.NewsletterUnsubscribed);

        Assert.Equal(AdresseAwa, confirmation.Recipient);
    }

    [Fact]
    public async Task UnDesabonnementAvecMauvaiseCleEstIgnore()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);

        await DesabonnerAsync("cle-inventee");

        Assert.Equal(NewsletterStatus.Confirmed, (await AbonnementAsync(AdresseAwa)).Status);
    }

    [Fact]
    public async Task UnAbonneDesabonneNeRecoitPlusLaLettre()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);
        await SabonnerEtConfirmerAsync(AdresseMoussa);
        await DesabonnerAsync(CleDesabonnement(await AbonnementAsync(AdresseAwa)));

        var envoyes = await DiffuserAsync();

        Assert.Equal(1, envoyes.QueuedCount);
        Assert.Equal(AdresseMoussa, Assert.Single(await LettresEnFileAsync()).Recipient);
    }

    [Fact]
    public async Task SeReabonnerRedemandeUneConfirmation()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);
        var premierJeton = await JetonDeConfirmationAsync();
        await DesabonnerAsync(CleDesabonnement(await AbonnementAsync(AdresseAwa)));

        await SabonnerAsync(AdresseAwa);

        // Se réabonner doit repasser par la confirmation, sinon une adresse désabonnée
        // reviendrait dans la liste sans l'avoir voulu.
        Assert.Equal(NewsletterStatus.Pending, (await AbonnementAsync(AdresseAwa)).Status);
        Assert.NotEqual(premierJeton, await JetonDeConfirmationAsync());
    }

    [Fact]
    public async Task UnAncienJetonDeConfirmationNeFonctionnePlus()
    {
        await SabonnerAsync(AdresseAwa);
        var premierJeton = await JetonDeConfirmationAsync();
        await SabonnerAsync(AdresseAwa);

        // La seconde demande a renouvelé le jeton : l'ancien lien ne doit plus confirmer,
        // sinon un lien intercepté resterait valable.
        await ConfirmerAsync(premierJeton);

        Assert.Equal(NewsletterStatus.Pending, (await AbonnementAsync(AdresseAwa)).Status);
    }

    [Fact]
    public async Task RedemanderUneAdresseDejaConfirmeeNEnvoieRien()
    {
        await SabonnerEtConfirmerAsync(AdresseAwa);
        var avant = (await MessagesEnFileAsync()).Count;

        await SabonnerAsync(AdresseAwa);

        // Répondre à cet appel révélerait qui lit la lettre : on ne renvoie donc aucun
        // nouveau lien à une adresse déjà confirmée.
        Assert.Equal(avant, (await MessagesEnFileAsync()).Count);
    }

    [Fact]
    public async Task LAdministrationListeLesAbonnements()
    {
        await SabonnerAsync(AdresseAwa);
        await SabonnerEtConfirmerAsync(AdresseMoussa);

        var tous = await ListerAsync();
        Assert.Equal(2, tous.TotalCount);

        var confirmes = await ListerAsync(NewsletterStatus.Confirmed);
        Assert.Equal(AdresseMoussa, Assert.Single(confirmes.Items).Email);
    }

    [Fact]
    public async Task UnMembreNeListePasLesAbonnes()
    {
        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => ListerAsync(utilisateur: UtilisateurFictif.Membre(Guid.NewGuid())));

        Assert.Equal("forbidden", erreur.Code);
    }

    [Fact]
    public async Task UnMembreNeDiffusePasLaLettre()
    {
        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => DiffuserAsync(UtilisateurFictif.Membre(Guid.NewGuid())));

        Assert.Equal("forbidden", erreur.Code);
    }

    private static string CleDesabonnement(NewsletterSubscription abonnement) =>
        string.IsNullOrWhiteSpace(abonnement.UnsubscribeKey)
            ? throw new InvalidOperationException("L'abonnement n'a pas de clé de désabonnement.")
            : abonnement.UnsubscribeKey;

    private async Task<NewsletterSendResponse> DiffuserAsync(ICurrentUserService? utilisateur = null)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = CreerControleurAdmin(portee, utilisateur);
        var resultat = await controleur.Diffuser(RequeteDeDiffusion(), default);

        return Assert.IsType<NewsletterSendResponse>(
            Assert.IsType<AcceptedResult>(resultat.Result).Value);
    }

    private static SendNewsletterRequest RequeteDeDiffusion() =>
        new(
            "Événement",
            "Rencontre .NET de Niamey",
            "Une soirée d'échange sur ASP.NET Core.",
            "https://dotnetniger.test/evenements/rencontre-dotnet",
            "Awa Traoré");

    private async Task SabonnerEtConfirmerAsync(string email)
    {
        await SabonnerAsync(email);
        await ConfirmerAsync(await JetonDeConfirmationAsync());
    }

    private async Task SabonnerAsync(string email)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = CreerControleur(portee, null);
        var resultat = await controleur.Sabonner(new SubscribeToNewsletterRequest(email), default);

        Assert.Equal(StatusCodes.Status202Accepted, Assert.IsType<AcceptedResult>(resultat).StatusCode);
    }

    private async Task ConfirmerAsync(string jeton)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = CreerControleur(portee, null);
        await controleur.Confirmer(jeton, default);
    }

    private async Task DesabonnerAsync(string cle)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = CreerControleur(portee, null);
        await controleur.Desabonner(cle, default);
    }

    private async Task<PageResponse<NewsletterSubscriptionResponse>> ListerAsync(
        NewsletterStatus? statut = null,
        ICurrentUserService? utilisateur = null)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = CreerControleurAdmin(portee, utilisateur);
        var resultat = await controleur.Lister(statut, 1, 20, default);

        return Assert.IsType<PageResponse<NewsletterSubscriptionResponse>>(resultat.Value);
    }

    private static NewsletterController CreerControleur(IServiceScope portee, ICurrentUserService? utilisateur) =>
        new(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            portee.ServiceProvider.GetRequiredService<IEmailDispatcher>(),
            portee.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>(),
            TimeProvider.System,
            NullLogger<NewsletterController>.Instance);

    private static AdminNewsletterController CreerControleurAdmin(
        IServiceScope portee,
        ICurrentUserService? utilisateur) =>
        new(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            utilisateur ?? UtilisateurFictif.Administrateur(Guid.NewGuid()),
            portee.ServiceProvider.GetRequiredService<IEmailDispatcher>(),
            portee.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>(),
            NullLogger<AdminNewsletterController>.Instance);

    private DotnetNigerDbContext Contexte =>
        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

    private async Task<List<NewsletterSubscription>> AbonnementsAsync() =>
        await Contexte.NewsletterSubscriptions.AsNoTracking().ToListAsync();

    private async Task<NewsletterSubscription> AbonnementAsync(string email) =>
        Assert.Single(
            await Contexte.NewsletterSubscriptions
                .AsNoTracking()
                .Where(abonnement => abonnement.Email == email)
                .ToListAsync());

    private async Task<List<OutboxMessage>> MessagesEnFileAsync() =>
        await Contexte.OutboxMessages.AsNoTracking().ToListAsync();

    /// <summary>
    /// Ne garde que les envois de la lettre elle-même : la file contient aussi les
    /// confirmations d'abonnement et de désabonnement, qui ne disent rien de la diffusion.
    /// </summary>
    private async Task<List<OutboxMessage>> LettresEnFileAsync() =>
        [.. (await MessagesEnFileAsync())
            .Where(message => message.Kind == EmailTemplateKeys.NewsletterNewContent)];

    private async Task<string> JetonDeConfirmationAsync()
    {
        // Le jeton n'est stocké que sous forme d'empreinte : on le relit dans le courriel
        // mis en file, exactement comme le ferait la personne qui clique sur le lien.
        // Le plus récent : c'est le jeton de la demande en cours, pas celui d'une plus
        // ancienne qui aurait pu rester valable.
        var message = await Contexte.OutboxMessages
            .AsNoTracking()
            .Where(m => m.Kind == EmailTemplateKeys.NewsletterConfirmation)
            .OrderByDescending(m => m.Id)
            .FirstAsync();

        var correspondance = JetonDansLeLien().Match(message.HtmlBody);

        Assert.True(correspondance.Success, "Le courriel de confirmation ne contient aucun jeton.");

        return correspondance.Groups[1].Value;
    }

    [GeneratedRegex(@"token=([A-Za-z0-9\-_]+)")]
    private static partial Regex JetonDansLeLien();

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
