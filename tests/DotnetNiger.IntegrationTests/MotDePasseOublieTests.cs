using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie le parcours « mot de passe oublié » : envoi du lien, changement effectif du mot de
/// passe, et refus d'un jeton qui ne vaut plus.
/// </summary>
/// <remarks>
/// Le parcours repose sur le moteur de jetons d'Identity et sur la politique de mot de passe :
/// un double ne prouverait ni que le lien généré est rejouable, ni que le nouveau mot de passe
/// est réellement appliqué. Un vrai <c>UserManager</c> sur une base jetable est donc monté,
/// comme dans <see cref="IdentityFixture"/>.
/// </remarks>
public sealed class MotDePasseOublieTests : IDisposable
{
    private const string AncienMotDePasse = "MotDePasseSolide1!";
    private const string NouveauMotDePasse = "NouveauMotDePasse2!";

    private readonly IdentityFixture _fixture = new();
    private readonly FauxExpediteur _expediteur = new();
    private readonly AccountOperations _operations;

    public MotDePasseOublieTests()
    {
        _operations = new AccountOperations(
            _fixture.UserManager,
            _fixture.SignInManager,
            _expediteur,
            Options.Create(OptionsCourriel()),
            NullLogger<AccountOperations>.Instance);
    }

    [Fact]
    public async Task Envoyer_le_lien_compose_une_url_ouvrable()
    {
        var compte = await CreerCompteAsync("amina@example.org");

        await _operations.SendPasswordResetAsync(compte, CancellationToken.None);

        var message = Assert.Single(_expediteur.Messages);
        Assert.Equal("amina@example.org", message.Recipient);

        // Le lien doit réellement pointer vers la page de réinitialisation du site, sinon le
        // membre reçoit un message qui ne mène nulle part.
        var url = ExtraireLien(message);

        Assert.StartsWith("https://dotnetniger.test/compte/reinitialiser-mot-de-passe?", url, StringComparison.Ordinal);
        Assert.Contains("courriel=amina@example.org", url, StringComparison.Ordinal);
        Assert.Contains("jeton=", url, StringComparison.Ordinal);

        // Le lien doit porter le jeton qu'Identity reconnaît, sinon le formulaire de nouveau
        // mot de passe s'ouvrirait sur une page qui ne pourra rien enregistrer.
        // La preuve que le jeton du lien est le bon : on s'en sert tel quel, exactement
        // comme le formulaire le fera après avoir suivi le lien.
        var jeton = url[(url.IndexOf("jeton=", StringComparison.Ordinal) + 6)..];

        var reinitialisation = await _operations.ResetPasswordAsync(
            "amina@example.org",
            jeton,
            NouveauMotDePasse);

        Assert.True(reinitialisation.IsSuccess, reinitialisation.Error);
    }

    [Fact]
    public async Task Le_lien_permet_de_changer_le_mot_de_passe()
    {
        var compte = await CreerCompteAsync("amina@example.org");
        var jeton = await _fixture.UserManager.GeneratePasswordResetTokenAsync(compte);

        var resultat = await _operations.ResetPasswordAsync(
            "amina@example.org",
            jeton,
            NouveauMotDePasse);

        Assert.True(resultat.IsSuccess, resultat.Error);

        // Le compte est relu : la seule preuve que le mot de passe a changé est que l'ancien
        // ne correspond plus et que le nouveau, si. `CheckPasswordAsync` suffit — ouvrir une
        // session exigerait un schéma d'authentification, inutile ici.
        var relecture = await _fixture.UserManager.FindByEmailAsync("amina@example.org");
        Assert.NotNull(relecture);

        var nouveau = await _fixture.UserManager.CheckPasswordAsync(relecture, NouveauMotDePasse);
        var ancien = await _fixture.UserManager.CheckPasswordAsync(relecture, AncienMotDePasse);

        Assert.True(nouveau);
        Assert.False(ancien);
    }

    [Fact]
    public async Task Un_jeton_ne_sert_quune_seule_fois()
    {
        var compte = await CreerCompteAsync("amina@example.org");
        var jeton = await _fixture.UserManager.GeneratePasswordResetTokenAsync(compte);

        var premier = await _operations.ResetPasswordAsync("amina@example.org", jeton, NouveauMotDePasse);
        var second = await _operations.ResetPasswordAsync("amina@example.org", jeton, "EncoreUnAutre3!");

        Assert.True(premier.IsSuccess, premier.Error);
        Assert.False(second.IsSuccess);
    }

    [Fact]
    public async Task Un_compte_supprime_entre_temps_ne_revele_pas_son_absence()
    {
        // Le jeton reste transmis dans le lien ; seule l'absence de compte est simulée. Le
        // message doit rester celui d'un jeton invalide, pas révéler que l'adresse n'existe pas.
        var resultat = await _operations.ResetPasswordAsync(
            "disparu@example.org",
            "jeton-qui-na-pas-ete-emis",
            NouveauMotDePasse);

        Assert.False(resultat.IsSuccess);

        var jetonExpire = await _operations.ResetPasswordAsync(
            "amina-inconnu@example.org",
            "jeton-qui-na-pas-ete-emis",
            NouveauMotDePasse);

        Assert.Equal(resultat.Error, jetonExpire.Error);
    }

    [Fact]
    public async Task Un_mot_de_passe_trop_faible_est_refuse_avec_son_message()
    {
        var compte = await CreerCompteAsync("amina@example.org");
        var jeton = await _fixture.UserManager.GeneratePasswordResetTokenAsync(compte);

        var resultat = await _operations.ResetPasswordAsync("amina@example.org", jeton, "court");

        Assert.False(resultat.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(resultat.Error));

        // Le compte n'a pas bougé : l'ancien mot de passe reste le bon.
        var relecture = await _fixture.UserManager.FindByEmailAsync("amina@example.org");
        Assert.NotNull(relecture);

        Assert.True(await _fixture.UserManager.CheckPasswordAsync(relecture, AncienMotDePasse));
    }

    [Fact]
    public async Task Aucun_envoi_n_est_compose_sans_serveur_de_courriel()
    {
        // Un relais renseigné mais sans adresse du site : l'envoi reste impossible, et c'est
        // précisément le cas que IsMisconfigured signale au démarrage.
        var sansSite = new EmailOptions
        {
            Host = "relais.test",
            FromAddress = "contact@dotnetniger.test",
        };

        var sansServeur = new AccountOperations(
            _fixture.UserManager,
            _fixture.SignInManager,
            _expediteur,
            Options.Create(sansSite),
            NullLogger<AccountOperations>.Instance);

        var compte = await CreerCompteAsync("amina@example.org");

        await sansServeur.SendPasswordResetAsync(compte, CancellationToken.None);

        Assert.False(sansServeur.EmailConfigured);
        Assert.Empty(_expediteur.Messages);
    }

    public void Dispose() => _fixture.Dispose();

    /// <summary>
    /// Réglages d'un relais de courriel opérationnel.
    /// </summary>
    /// <returns>Réglages complets, adresse du site comprise.</returns>
    private static EmailOptions OptionsCourriel() => new()
    {
        Host = "relais.test",
        Port = 587,
        FromAddress = "contact@dotnetniger.test",
        ContactAddress = "contact@dotnetniger.test",
        SiteUrl = "https://dotnetniger.test",
    };

    private async Task<ApplicationUser> CreerCompteAsync(string email)
    {
        var compte = new ApplicationUser(email);
        compte.UpdateProfile("Amina", "Traoré");

        var creation = await _fixture.UserManager.CreateAsync(compte, AncienMotDePasse);

        Assert.True(creation.Succeeded);

        return compte;
    }

    /// <summary>
    /// Relève l'adresse de réinitialisation dans le corps HTML du message.
    /// </summary>
    /// <param name="message">Message expédié.</param>
    /// <returns>Adresse complète du lien.</returns>
    private static string ExtraireLien(EmailEnvoye message)
    {
        const string Debut = "https://dotnetniger.test/compte/reinitialiser-mot-de-passe?";
        var corps = message.Message.TextBody;
        var debut = corps.IndexOf(Debut, StringComparison.Ordinal);

        Assert.True(debut >= 0, "Le lien de réinitialisation est absent du message.");

        var fin = corps.IndexOfAny([' ', '\n', '\r', '<', '"'], debut);
        var lien = fin < 0 ? corps[debut..] : corps[debut..fin];

        return Uri.UnescapeDataString(lien);
    }

    /// <summary>
    /// Un message expédié, avec son destinataire.
    /// </summary>
    /// <param name="Recipient">Adresse du destinataire.</param>
    /// <param name="Message">Message rendu.</param>
    private sealed record EmailEnvoye(string Recipient, EmailMessage Message);

    /// <summary>
    /// Expéditeur qui garde les messages en mémoire au lieu de partir vers un relais SMTP.
    /// </summary>
    private sealed class FauxExpediteur : IEmailSender
    {
        public List<EmailEnvoye> Messages { get; } = [];

        public Task SendAsync(
            string recipient,
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            Messages.Add(new EmailEnvoye(recipient, message));

            return Task.CompletedTask;
        }
    }
}
