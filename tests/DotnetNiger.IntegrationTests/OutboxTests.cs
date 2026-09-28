using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Notifications;
using DotnetNiger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie la file d'attente de bout en bout : ce qui entre dans la file part, ce qui échoue
/// revient, et ce qui échoue trop longtemps est abandonné sans bloquer les autres.
/// </summary>
/// <remarks>
/// Le relais de fond est désactivé dans ces tests : ils appellent
/// <see cref="OutboxProcessor.DrainAsync"/> une fois, à un instant choisi, ce qui rend
/// vérifiables la programmation des tentatives. Le vrai <c>BackgroundService</c> ne fait
/// qu'appeler cette méthode en boucle.
/// </remarks>
public sealed class OutboxTests : IDisposable
{
    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly HorlogeMobile _horloge;
    private readonly EmailSenderFictif _expediteur = new();
    private readonly OutboxOptions _options = new() { MaxAttempts = 3, BatchSize = 100 };

    public OutboxTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-outbox-{Guid.NewGuid():N}.db");

        _horloge = new HorlogeMobile(new DateTimeOffset(2026, 9, 27, 10, 0, 0, TimeSpan.Zero));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_cheminBase}",
                ["Database:Provider"] = "Sqlite",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_horloge);
        services.AddSingleton<IEmailSender>(_expediteur);
        services.AddScoped<ICurrentUserService>(_ => UtilisateurFictif.Administrateur(Guid.NewGuid()));
        services.AddDotnetNigerPersistence(configuration);

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task UnCourrielMisEnFileNExisteQuAuValiderLaTransaction()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "membre@example.test", "Bienvenue");

        // Tant que l'appelant n'a pas validé, le message n'est qu'une écriture en attente :
        // c'est ce qui garantit qu'il ne partira pas si le changement métier est annulé.
        Assert.Equal(0, await CompterEnBaseAsync());

        await UnitOfWork().SaveChangesAsync();

        Assert.Equal(1, await CompterEnBaseAsync());
    }

    [Fact]
    public async Task UnMessageEnFileEstExpedieAuTourSuivant()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "membre@example.test", "Bienvenue");
        await UnitOfWork().SaveChangesAsync();

        var traites = await Processeur().DrainAsync();

        Assert.Equal(1, traites);
        Assert.Equal("membre@example.test", Assert.Single(_expediteur.Envoyes).Recipient);

        var message = await ChargerMessageAsync();
        Assert.Equal(OutboxMessageStatus.Sent, message.Status);
        Assert.NotNull(message.SentAtUtc);
    }

    [Fact]
    public async Task UneDiffusionMetUnMessageParAbonne()
    {
        var diffusion = Guid.NewGuid();

        await Outbox().EnqueueManyAsync(
            EmailTemplateKeys.NewsletterNewContent,
            [
                new QueuedEmail("a@example.test", new EmailMessage("Nouveauté", "<p>a</p>", "a")),
                new QueuedEmail("b@example.test", new EmailMessage("Nouveauté", "<p>b</p>", "b")),
                new QueuedEmail("c@example.test", new EmailMessage("Nouveauté", "<p>c</p>", "c")),
            ],
            diffusion);

        await UnitOfWork().SaveChangesAsync();

        var messages = await _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>()
            .OutboxMessages
            .Where(message => message.BroadcastId == diffusion)
            .ToListAsync();

        Assert.Equal(3, messages.Count);

        // Chaque abonné a son propre corps : c'est ce qui permet un lien de désinscription
        // individuel, exigé pour une diffusion commerciale.
        Assert.Equal(3, messages.Select(message => message.HtmlBody).Distinct().Count());
    }

    [Fact]
    public async Task UneDiffusionVideNeMetRienEnFile()
    {
        var mis = await Outbox().EnqueueManyAsync(
            EmailTemplateKeys.NewsletterNewContent,
            [],
            Guid.NewGuid());

        await UnitOfWork().SaveChangesAsync();

        Assert.Equal(0, mis);
        Assert.Equal(0, await CompterEnBaseAsync());
    }

    [Fact]
    public async Task UnEchecReprogrammeUneTentativeSansPerdreLeMessage()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "membre@example.test", "Bienvenue");
        await UnitOfWork().SaveChangesAsync();

        _expediteur.ProchainEnvoiEchoue();

        await Processeur().DrainAsync();

        var message = await ChargerMessageAsync();
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(1, message.Attempts);
        Assert.Contains("joignable", message.LastError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(_horloge.Instant.UtcDateTime.Add(OutboxMessage.RetryDelay), message.NextAttemptAtUtc);
    }

    [Fact]
    public async Task UnMessageEnAttenteNeRetardePasLesSuivants()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "premier@example.test", "Premier");
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "deuxieme@example.test", "Deuxième");
        await UnitOfWork().SaveChangesAsync();

        // Le premier message vient d'échouer et repartira plus tard. Le relais tourne en
        // continu : il ne doit pas attendre son heure pour expedier le message suivant.
        var messages = await ChargerMessagesAsync();
        var premier = messages.Single(message => message.Recipient == "premier@example.test");
        premier.ScheduleRetry("SMTP injoignable", _options.MaxAttempts, _horloge.Instant.UtcDateTime);
        await UnitOfWork().SaveChangesAsync();

        var traites = await Processeur().DrainAsync();

        Assert.Equal(1, traites);
        Assert.Equal("deuxieme@example.test", Assert.Single(_expediteur.Envoyes).Recipient);
    }

    [Fact]
    public async Task UnMessageÉchouéPuisRéussiReprendSonExpédition()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "membre@example.test", "Bienvenue");
        await UnitOfWork().SaveChangesAsync();

        _expediteur.ProchainEnvoiEchoue();
        await Processeur().DrainAsync();

        _horloge.Avancer(OutboxMessage.RetryDelay);
        await Processeur().DrainAsync();

        var message = await ChargerMessageAsync();
        Assert.Equal(OutboxMessageStatus.Sent, message.Status);
        Assert.Null(message.LastError);
    }

    [Fact]
    public async Task UnMessageEstAbandonnéAprèsLeNombreDeTentativesPrévu()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "membre@example.test", "Bienvenue");
        await UnitOfWork().SaveChangesAsync();

        // Le serveur de courriel tombe et ne se relèvera pas : le message doit être
        // abandonné, et surtout pas réexpédié indéfiniment.
        _expediteur.EchecsPermanents();

        for (var essai = 0; essai < _options.MaxAttempts; essai++)
        {
            await Processeur().DrainAsync();
            _horloge.Avancer(OutboxMessage.MaxRetryDelay);
        }

        var message = await ChargerMessageAsync();
        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Null(message.NextAttemptAtUtc);

        // Une fois abandonné, il n'est plus repris et ne bloque pas la file.
        Assert.Equal(0, await Processeur().DrainAsync());
    }

    [Fact]
    public async Task UnMessageExpediéNEstPasRéexpédié()
    {
        await MettreEnFileAsync(EmailTemplateKeys.AccountWelcome, "membre@example.test", "Bienvenue");
        await UnitOfWork().SaveChangesAsync();

        await Processeur().DrainAsync();
        var traites = await Processeur().DrainAsync();

        Assert.Equal(0, traites);
        Assert.Single(_expediteur.Envoyes);
    }

    private IUnitOfWork UnitOfWork() => _portee.ServiceProvider.GetRequiredService<IUnitOfWork>();

    private IOutbox Outbox() => _portee.ServiceProvider.GetRequiredService<IOutbox>();

    private OutboxProcessor Processeur() => new(
        UnitOfWork(),
        _expediteur,
        _horloge,
        Microsoft.Extensions.Options.Options.Create(_options),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<OutboxProcessor>.Instance);

    private async Task MettreEnFileAsync(string kind, string destinataire, string objet)
    {
        await Outbox().EnqueueAsync(
            kind,
            new QueuedEmail(destinataire, new EmailMessage(objet, $"<p>Bonjour {destinataire}</p>", $"Bonjour {destinataire}")));
    }

    /// <summary>
    /// Compte les lignes réellement présentes en base, sans vider le suivi : un
    /// <c>ChangeTracker.Clear</c> détacherait les écritures en attente et ferait échouer la
    /// transaction que ce test cherche justement à observer.
    /// </summary>
    private async Task<int> CompterEnBaseAsync()
    {
        return await _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>()
            .OutboxMessages
            .CountAsync();
    }

    /// <summary>
    /// Recharge les messages en suivant les entités : c'est la seule façon d'observer ce
    /// qui a réellement été écrit, et non l'état en mémoire du contexte.
    /// </summary>
    private async Task<List<OutboxMessage>> ChargerMessagesAsync()
    {
        var contexte = _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        contexte.ChangeTracker.Clear();

        return await contexte.OutboxMessages
            .OrderBy(message => message.Recipient)
            .ToListAsync();
    }

    private async Task<OutboxMessage> ChargerMessageAsync()
        => Assert.Single(await ChargerMessagesAsync());

    public void Dispose()
    {
        _portee.Dispose();
        _fournisseur.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        foreach (var fichier in new[] { _cheminBase, _cheminBase + "-shm", _cheminBase + "-wal" })
        {
            if (File.Exists(fichier))
            {
                File.Delete(fichier);
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Horloge que le test avance à volonté, pour vérifier le retour à l'heure prévue
    /// sans attendre réellement.
    /// </summary>
    private sealed class HorlogeMobile(DateTimeOffset instant) : TimeProvider
    {
        public DateTimeOffset Instant { get; private set; } = instant;

        public override DateTimeOffset GetUtcNow() => Instant;

        public void Avancer(TimeSpan delai) => Instant = Instant.Add(delai);
    }

    /// <summary>
    /// Expéditeur qui enregistre ce qu'il a expedié et peut faire échouer l'envoi suivant.
    /// </summary>
    private sealed class EmailSenderFictif : IEmailSender
    {
        private bool _echecProchain;
        private bool _echecsPermanents;

        public List<(string Recipient, EmailMessage Message)> Envoyes { get; } = [];

        public void ProchainEnvoiEchoue() => _echecProchain = true;

        public void EchecsPermanents() => _echecsPermanents = true;

        public Task SendAsync(
            string recipient,
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            if (_echecsPermanents || _echecProchain)
            {
                _echecProchain = false;
                throw new EmailDeliveryException("SMTP injoignable");
            }

            Envoyes.Add((recipient, message));
            return Task.CompletedTask;
        }
    }
}
