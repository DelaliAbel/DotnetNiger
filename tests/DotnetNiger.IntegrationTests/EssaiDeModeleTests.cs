using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Notifications;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie l'essai d'un modèle de courriel depuis l'administration.
/// </summary>
/// <remarks>
/// L'enjeu de cet essai n'est pas l'envoi — il n'y a pas de serveur de courriel ici — mais
/// ses garde-fous : la destination est imposée par le compte connecté, le message est
/// inscrit avant que l'appelant ne soit renseigné, et une clé inconnue ne se transforme
/// pas en envoi vide.
/// </remarks>
public sealed class EssaiDeModeleTests : IDisposable
{
    private const string AdresseAdmin = "admin@dotnetniger.test";

    private readonly string _cheminBase;
    private readonly ServiceProvider _fournisseur;
    private readonly IServiceScope _portee;
    private readonly Guid _adminId;

    public EssaiDeModeleTests()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-essai-{Guid.NewGuid():N}.db");

        _adminId = Guid.NewGuid();

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

        // La destination est déduite de l'adresse du compte, laquelle vient d'Identity :
        // l'essai n'a donc besoin d'aucune adresse fournie par l'appelant.
        services.Configure<EmailOptions>(options =>
        {
            options.SiteUrl = "https://dotnetniger.test";
            options.ContactAddress = "contact@dotnetniger.test";
        });

        // L'intercepteur d'audit résout l'utilisateur courant au moment du SaveChanges :
        // sans enregistrement, l'écriture elle-même échouerait. Le contrôleur reçoit
        // ci-dessous l'instance qu'on veut lui imposer, ce qui permet d'inverser les rôles
        // sans toucher au conteneur.
        services.AddScoped<ICurrentUserService>(_ => UtilisateurFictif.Administrateur(_adminId));

        services.AddDotnetNigerPersistence(configuration);

        // Identity résout ses fournisseurs de jetons dès la construction du UserManager ;
        // Data Protection est ajouté implicitement par l'hôte web, il doit donc l'être
        // explicitement dans un conteneur monté à la main.
        services.AddDataProtection();

        services
            .AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<DotnetNigerDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        _fournisseur = services.BuildServiceProvider();
        _portee = _fournisseur.CreateScope();

        var contexte = Contexte;
        contexte.Database.EnsureCreated();

        contexte.Users.Add(new ApplicationUser
        {
            Id = _adminId,
            UserName = AdresseAdmin,
            Email = AdresseAdmin,
            NormalizedEmail = AdresseAdmin.ToUpperInvariant(),
            NormalizedUserName = AdresseAdmin.ToUpperInvariant(),
            EmailConfirmed = true,
        });
        contexte.SaveChanges();
    }

    [Fact]
    public async Task UnEssaiEstMisEnFilePourLEresseDuCompteConnecte()
    {
        var (resultat, accuse) = await EssayerAsync(EmailTemplateKeys.AccountWelcome);

        Assert.Equal(StatusCodes.Status202Accepted, StatusDe(resultat));
        Assert.Equal(EmailTemplateKeys.AccountWelcome, accuse!.Cle);
        Assert.Equal(AdresseAdmin, accuse.Destinataire);
        Assert.False(string.IsNullOrWhiteSpace(accuse.Objet));
        Assert.False(string.IsNullOrWhiteSpace(accuse.Texte));

        // Le corps texte fait partie de la réponse : relire l'essai sans ouvrir une boîte
        // mail évite de dépendre du client de messagerie pour vérifier le rendu.
        Assert.Contains(accuse.Texte, await CorpsEnFileAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnEssaiEcritLeMessageAvantDeRepondre()
    {
        await EssayerAsync(EmailTemplateKeys.AccountWelcome);

        var message = Assert.Single(await MessagesEnFileAsync());

        Assert.Equal(EmailTemplateKeys.AccountWelcome, message.Kind);
        Assert.Equal(AdresseAdmin, message.Recipient);

        // Les deux corps ont survécu au passage par la file : c'est ce qui permet à un
        // membre lisant en texte brut de ne rien perdre du message.
        Assert.False(string.IsNullOrWhiteSpace(message.HtmlBody));
        Assert.False(string.IsNullOrWhiteSpace(message.TextBody));
    }

    [Fact]
    public async Task UnEssaiResteEnAttenteDEnvion()
    {
        await EssayerAsync(EmailTemplateKeys.AccountWelcome);

        // Rien n'est « envoyé » : le message attend son tour. L'endpoint ne prétend pas
        // avoir livré, il promet d'avoir inscrit.
        Assert.Equal(OutboxMessageStatus.Pending, Assert.Single(await MessagesEnFileAsync()).Status);
    }

    [Fact]
    public async Task UneCleInconnueEstRefuseeSansEcrire()
    {
        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => EssayerAsync("inconnu.modele"));

        Assert.Equal("not_found", erreur.Code);
        Assert.Empty(await MessagesEnFileAsync());
    }

    [Fact]
    public async Task UnMembreNePeutPasEssayerUnModele()
    {
        // L'essai ne doit pas devenir un moyen de faire sortir du courrier du système au
        // nom de l'administration. Le rôle est contrôlé avant tout accès au catalogue.
        var erreur = await Assert.ThrowsAsync<DomainException>(
            () => EssayerAsync(EmailTemplateKeys.AccountWelcome, UtilisateurFictif.Membre(Guid.NewGuid())));

        Assert.Equal("forbidden", erreur.Code);
        Assert.Empty(await MessagesEnFileAsync());
    }

    [Fact]
    public async Task ChaqueModeleDuCataloguePeutEtreEssaye()
    {
        // Un modèle qui casse au rendu ne doit pas être découvert le jour où l'on veut
        // l'envoyer : le catalogue entier passe ici, une clé après l'autre.
        foreach (var modele in EmailTemplateCatalog.Tous)
        {
            var (resultat, accuse) = await EssayerAsync(modele.Cle);

            Assert.Equal(StatusCodes.Status202Accepted, StatusDe(resultat));
            Assert.Equal(modele.Cle, accuse!.Cle);
        }

        Assert.Equal(EmailTemplateCatalog.Tous.Count, (await MessagesEnFileAsync()).Count);
    }

    private static int StatusDe(ActionResult<EmailTestQueuedResponse> resultat) =>
        Assert.IsType<AcceptedResult>(resultat.Result).StatusCode!.Value;

    private async Task<(ActionResult<EmailTestQueuedResponse> Resultat, EmailTestQueuedResponse? Accuse)> EssayerAsync(
        string cle,
        ICurrentUserService? utilisateur = null)
    {
        using var portee = _fournisseur.CreateScope();
        var services = portee.ServiceProvider;

        var controleur = new AdminEmailTemplatesController(
            services.GetRequiredService<IUnitOfWork>(),
            utilisateur ?? UtilisateurFictif.Administrateur(_adminId),
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<IOutbox>(),
            services.GetRequiredService<IOptions<EmailOptions>>(),
            NullLogger<AdminEmailTemplatesController>.Instance);

        var resultat = await controleur.EnvoyerEssai(cle, default);

        // Accepted(valeur) range la charge utile dans le résultat, pas dans Value : c'est
        // le chemin que parcourt le pipeline MVC, on le suit donc tel quel.
        return (resultat, Assert.IsType<AcceptedResult>(resultat.Result).Value as EmailTestQueuedResponse);
    }

    private async Task<string> CorpsEnFileAsync() =>
        Assert.Single(await MessagesEnFileAsync()).TextBody;

    private DotnetNigerDbContext Contexte =>
        _portee.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

    private async Task<List<OutboxMessage>> MessagesEnFileAsync() =>
        await Contexte.OutboxMessages
            .AsNoTracking()
            .OrderBy(message => message.Recipient)
            .ToListAsync();

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
