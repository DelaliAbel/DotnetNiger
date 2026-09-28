using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Notifications;
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
/// Vérifie le formulaire de contact et la boîte de réception de l'administration.
/// </summary>
/// <remarks>
/// L'enjeu est la promesse faite au visiteur : son message est gardé et un accusé part,
/// que le courriel soit en fin de non-recevoir. D'où les deux messages attendus — l'accusé
/// au demandeur et la copie à l'administration — et la conservation en base, seule trace
/// qui reste quand les deux courriels disparaissent.
/// </remarks>
public sealed class ContactTests : IDisposable
{
    private const string AdresseAwa = "awa.traore@exemple.test";

    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly string? _adresseAdmin;

    public ContactTests()
        : this(adresseAdmin: "contact@dotnetniger.test")
    {
    }

    private ContactTests(string? adresseAdmin)
    {
        _adresseAdmin = adresseAdmin;

        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-contact-{Guid.NewGuid():N}.db");

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
            options.ContactAddress = _adresseAdmin ?? string.Empty;
        });

        services.AddScoped<ICurrentUserService>(
            _ => UtilisateurFictif.Administrateur(Guid.NewGuid()));

        services.AddDotnetNigerPersistence(configuration);

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        Contexte.Database.EnsureCreated();
    }

    /// <summary>
    /// Construit un test sans adresse d'administration, pour vérifier que cette absence
    /// n'empêche pas de recevoir les messages des visiteurs.
    /// </summary>
    /// <returns>Un test prêt à être disposé.</returns>
    public static ContactTests SansAdresseAdmin() => new(adresseAdmin: null);

    [Fact]
    public async Task UnMessageEstDeposeEtConserveEnAttente()
    {
        var resultat = await DeposerAsync();

        Assert.Equal(StatusCodes.Status201Created, StatutDe(resultat));

        var message = Assert.Single(await MessagesAsync());
        Assert.Equal("Bonjour, pouvez-vous animer un atelier C# ?", message.Body);
        Assert.Equal("Proposition d'atelier", message.Subject);
        Assert.Null(message.HandledAtUtc);
    }

    [Fact]
    public async Task LeDemandeurEtLAdministrationRecoiventChacunUnCourriel()
    {
        await DeposerAsync();

        var enFile = await MessagesEnFileAsync();

        // Deux destinataires distincts : confondre les deux enverrait l'accusé à
        // l'administration, ou ne confirmationnerait personne du tout.
        var accuse = enFile.Single(message => message.Kind == EmailTemplateKeys.ContactAcknowledged);
        var copie = enFile.Single(message => message.Kind == EmailTemplateKeys.ContactForwarded);

        Assert.Equal(AdresseAwa, accuse.Recipient);
        Assert.Equal(_adresseAdmin, copie.Recipient);
        Assert.Equal(2, enFile.Count);
    }

    [Fact]
    public async Task LAccuseReprendLeMessageRecu()
    {
        await DeposerAsync("Demande de bénévolat", "Je peux aider le samedi suivant.");

        var accuse = (await MessagesEnFileAsync())
            .Single(message => message.Kind == EmailTemplateKeys.ContactAcknowledged);

        // Le visiteur doit pouvoir relire ce que l'on a bien reçu : sans cela, un envoi
        // tronqué reste invisible jusqu'à la réponse.
        Assert.Contains("Je peux aider le samedi suivant.", accuse.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SansAdresseAdminLeMessageEstRecuEtAccuse()
    {
        using var test = SansAdresseAdmin();

        await test.DeposerAsync();

        // L'absence d'adresse d'administration ne doit rien coûter au visiteur : ni son
        // message, ni son accusé. L'administration retrouvera tout dans la boîte.
        Assert.Single(await test.MessagesEnFileAsync());
        Assert.Single(await test.MessagesAsync());
    }

    [Fact]
    public async Task LaBoiteTrieDuPlusRecentAuPlusAncien()
    {
        await DeposerAsync("Premier sujet", "Le tout premier message");
        await DeposerAsync("Deuxième sujet", "Le suivant");
        await DeposerAsync("Troisième sujet", "Le dernier reçu");

        var liste = await ListerAsync();

        Assert.Equal(3, liste.TotalCount);
        Assert.Equal("Troisième sujet", liste.Items[0].Subject);
        Assert.Equal("Premier sujet", liste.Items[2].Subject);
    }

    [Fact]
    public async Task UnMembreNeConsultePasLaBoite()
    {
        await DeposerAsync();

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => ListerAsync(utilisateur: UtilisateurFictif.Membre(Guid.NewGuid())));

        Assert.Equal("forbidden", erreur.Code);
    }

    [Fact]
    public async Task UnMembreNeTraitePasUnMessage()
    {
        await DeposerAsync();
        var id = (await MessagesAsync())[0].Id;

        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => MarquerTraiteAsync(id, UtilisateurFictif.Membre(Guid.NewGuid())));

        Assert.Equal("forbidden", erreur.Code);
        Assert.Null((await MessagesAsync())[0].HandledAtUtc);
    }

    [Fact]
    public async Task UnMessageTraiteSortDesEnAttentes()
    {
        await DeposerAsync("Premier sujet", "Le premier");
        await DeposerAsync("Second sujet", "Le second");

        await MarquerTraiteAsync((await MessagesAsync())[0].Id);

        var enAttente = await ListerAsync(enAttente: true);

        // Le premier message est traité : il ne doit plus apparaître parmi les en attente.
        Assert.Single(enAttente.Items);
        Assert.Equal("Second sujet", enAttente.Items[0].Subject);
        Assert.Equal(2, (await ListerAsync()).TotalCount);
    }

    [Fact]
    public async Task UnMessageTraiteConserveLaDateDuTraitement()
    {
        await DeposerAsync();
        var id = (await MessagesAsync())[0].Id;

        var resultat = await MarquerTraiteAsync(id);

        var traite = (ContactMessageResponse)Assert.IsType<OkObjectResult>(resultat.Result).Value!;

        Assert.NotNull(traite.HandledAtUtc);
        Assert.Equal(traite.HandledAtUtc, (await MessagesAsync())[0].HandledAtUtc);
    }

    [Fact]
    public async Task UnMessageInconnuNEstPasTraite()
    {
        var resultat = await MarquerTraiteAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(resultat.Result);
    }

    [Fact]
    public async Task UnMessageRetireDisparaitDeLaBoite()
    {
        await DeposerAsync();
        var id = (await MessagesAsync())[0].Id;

        await SupprimerAsync(id);

        Assert.Empty((await ListerAsync()).Items);
    }

    [Fact]
    public async Task UnMessageDejaRetireEstIntrouvable()
    {
        await DeposerAsync();
        var id = (await MessagesAsync())[0].Id;

        await SupprimerAsync(id);

        var resultat = await SupprimerAsync(id);

        Assert.IsType<NotFoundResult>(resultat);
    }

    private static int StatutDe(ActionResult<ContactMessageResponse> resultat) =>
        Assert.IsType<CreatedResult>(resultat.Result).StatusCode!.Value;

    private async Task<ActionResult<ContactMessageResponse>> DeposerAsync(
        string sujet = "Proposition d'atelier",
        string corps = "Bonjour, pouvez-vous animer un atelier C# ?") =>
        await DeposerAsync(sujet, corps, null);

    private async Task<ActionResult<ContactMessageResponse>> DeposerAsync(
        string sujet,
        string corps,
        ICurrentUserService? utilisateur)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new ContactController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            portee.ServiceProvider.GetRequiredService<IEmailDispatcher>(),
            portee.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>(),
            TimeProvider.System,
            NullLogger<ContactController>.Instance);

        return await controleur.Deposer(
            new CreateContactMessageRequest("Awa Traoré", AdresseAwa, sujet, corps),
            default);
    }

    private async Task<PageResponse<ContactMessageResponse>> ListerAsync(
        bool enAttente = false,
        ICurrentUserService? utilisateur = null)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new AdminContactController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            utilisateur ?? UtilisateurFictif.Administrateur(Guid.NewGuid()),
            TimeProvider.System);

        var resultat = await controleur.Lister(1, 20, enAttente, default);

        return Assert.IsType<PageResponse<ContactMessageResponse>>(resultat.Value);
    }

    private async Task<ActionResult<ContactMessageResponse>> MarquerTraiteAsync(
        Guid id,
        ICurrentUserService? utilisateur = null)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new AdminContactController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            utilisateur ?? UtilisateurFictif.Administrateur(Guid.NewGuid()),
            TimeProvider.System);

        return await controleur.MarquerTraite(id, default);
    }

    private async Task<IActionResult> SupprimerAsync(Guid id)
    {
        using var portee = _fournisseur.CreateScope();

        var controleur = new AdminContactController(
            portee.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            UtilisateurFictif.Administrateur(Guid.NewGuid()),
            TimeProvider.System);

        return await controleur.Supprimer(id, default);
    }

    private DotnetNigerDbContext Contexte =>
        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

    private async Task<List<ContactMessage>> MessagesAsync() =>
        await Contexte.ContactMessages.AsNoTracking().OrderBy(m => m.Subject).ToListAsync();

    private async Task<List<OutboxMessage>> MessagesEnFileAsync() =>
        await Contexte.OutboxMessages.AsNoTracking().ToListAsync();

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
