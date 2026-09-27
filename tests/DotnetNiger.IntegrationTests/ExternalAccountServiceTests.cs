using System.Security.Claims;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie la règle centrale du site : une identité externe ne peut pas s'approprier un
/// compte existant sur la seule foi d'une adresse déclarée.
/// </summary>
public class ExternalAccountServiceTests : IDisposable
{
    private const string Provider = "Google";
    private const string ProviderKey = "google-1234";
    private const string Adresse = "membre@example.org";

    private readonly IdentityFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Claim[] RevendicationsCertifiees => [new Claim("email_verified", "true")];

    private static Claim[] RevendicationsNonCertifiees =>
        [new Claim(ClaimTypes.Email, Adresse)];

    [Fact]
    public async Task Complete_refuse_de_fusionner_un_compte_existant_non_certifie()
    {
        // Un compte local existe déjà pour cette adresse.
        var existant = new ApplicationUser(Adresse);
        await _fixture.UserManager.CreateAsync(existant, "MotDePasseLocal1!");

        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        var resultat = await service.Complete(
            Provider,
            ProviderKey,
            Adresse,
            RevendicationsNonCertifiees);

        // Le pire scénario : quelqu'un enregistre l'adresse d'un tiers chez Google, se
        // connecte, et récupère le compte. Le service doit exiger le mot de passe.
        Assert.Equal(ExternalSignInStatus.PasswordProofRequired, resultat.Status);
        Assert.False(resultat.CanSignIn);

        var logins = await _fixture.UserManager.GetLoginsAsync(existant);
        Assert.Empty(logins);
    }

    [Fact]
    public async Task Complete_refuse_la_session_meme_si_une_identite_existe_deja()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        var resultat = await service.Complete(
            Provider,
            ProviderKey,
            Adresse,
            RevendicationsNonCertifiees);

        // Aucun compte pour cette adresse : le membre doit d'abord choisir un mot de passe.
        Assert.Equal(ExternalSignInStatus.PasswordRequired, resultat.Status);
        Assert.False(resultat.CanSignIn);

        // Aucun compte ne doit avoir été créé en arrière-plan.
        Assert.Null(await _fixture.UserManager.FindByEmailAsync(Adresse));
    }

    [Fact]
    public async Task Complete_rattache_un_compte_existant_si_le_fournisseur_certifie_ladresse()
    {
        var existant = new ApplicationUser(Adresse);
        await _fixture.UserManager.CreateAsync(existant, "MotDePasseLocal1!");

        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        var resultat = await service.Complete(
            Provider,
            ProviderKey,
            Adresse,
            RevendicationsCertifiees);

        Assert.Equal(ExternalSignInStatus.LinkedToExistingAccount, resultat.Status);
        Assert.True(resultat.CanSignIn);

        var logins = await _fixture.UserManager.GetLoginsAsync(existant);
        Assert.Contains(logins, login => login.LoginProvider == Provider && login.ProviderKey == ProviderKey);
    }

    [Fact]
    public async Task Complete_exige_un_mot_de_passe_meme_pour_une_adresse_certifiee()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        var resultat = await service.Complete(
            Provider,
            ProviderKey,
            Adresse,
            RevendicationsCertifiees);

        // L'adresse est certifiée, mais cela ne dispense pas du mot de passe : c'est la règle
        // du site, et surtout c'est ce qui permet au membre de se connecter sans Google.
        Assert.Equal(ExternalSignInStatus.PasswordRequired, resultat.Status);
        Assert.False(resultat.CanSignIn);
        Assert.True(resultat.EmailVerified);

        // Aucun compte ne doit exister avant la saisie : un compte créé ici serait sans
        // PasswordHash, donc authentifiable uniquement par le fournisseur, et la saisie
        // ultérieure échouerait sur un doublon d'adresse.
        Assert.Null(await _fixture.UserManager.FindByEmailAsync(Adresse));
    }

    [Fact]
    public async Task Complete_puis_mot_de_passe_cree_un_compte_confirme_et_rattache()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        // 1. Le fournisseur certifie l'adresse, mais aucun compte n'existe.
        await service.Complete(Provider, ProviderKey, Adresse, RevendicationsCertifiees);

        // 2. Le membre choisit son mot de passe : le compte naît ici, complet.
        var creation = await service.CreateWithPasswordAsync(
            new ApplicationUser(Adresse),
            "MotDePasseSolide1!",
            Provider,
            ProviderKey,
            "Google");

        Assert.True(creation.Succeeded);

        var compte = await _fixture.UserManager.FindByEmailAsync(Adresse);
        Assert.NotNull(compte);
        Assert.True(await _fixture.UserManager.IsInRoleAsync(compte!, "Member"));
        Assert.Contains(
            await _fixture.UserManager.GetLoginsAsync(compte!),
            login => login.LoginProvider == Provider && login.ProviderKey == ProviderKey);

        // 3. Le compte est utilisable par mot de passe, et il est bien à l'adresse du membre.
        Assert.True(await _fixture.UserManager.CheckPasswordAsync(compte!, "MotDePasseSolide1!"));
    }

    [Fact]
    public async Task Complete_puis_mot_de_passe_ouvre_directement_une_seconde_session()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        await service.Complete(Provider, ProviderKey, Adresse, RevendicationsCertifiees);
        await service.CreateWithPasswordAsync(
            new ApplicationUser(Adresse),
            "MotDePasseSolide1!",
            Provider,
            ProviderKey,
            "Google");

        // Le compte existe et l'identité y est rattachée : le retour suivant se connecte
        // directement, sans redemander de mot de passe.
        var resultat = await service.Complete(Provider, ProviderKey, Adresse, RevendicationsCertifiees);

        Assert.Equal(ExternalSignInStatus.Connected, resultat.Status);
        Assert.True(resultat.CanSignIn);
    }

    [Fact]
    public async Task Complete_reconnait_directement_une_identite_deja_rattachee()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        await service.Complete(Provider, ProviderKey, Adresse, RevendicationsCertifiees);
        await service.CreateWithPasswordAsync(
            new ApplicationUser(Adresse),
            "MotDePasseSolide1!",
            Provider,
            ProviderKey,
            "Google");

        var resultat = await service.Complete(Provider, ProviderKey, Adresse, RevendicationsNonCertifiees);

        // L'identité est déjà connue : la certification n'est plus requise pour rouvrir une
        // session, sinon un membre perdrait l'accès après une réinitialisation chez le
        // fournisseur.
        Assert.Equal(ExternalSignInStatus.Connected, resultat.Status);
        Assert.True(resultat.CanSignIn);
    }

    [Fact]
    public async Task Complete_refuse_une_adresse_vide()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        var resultat = await service.Complete(Provider, ProviderKey, null, RevendicationsCertifiees);

        Assert.False(resultat.CanSignIn);
    }

    [Fact]
    public async Task CreateWithPasswordAsync_rattache_le_fournisseur_au_nouveau_compte()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());
        var user = new ApplicationUser("nouveau@example.org");

        var resultat = await service.CreateWithPasswordAsync(
            user,
            "MotDePasseSolide1!",
            Provider,
            ProviderKey,
            "Google");

        Assert.True(resultat.Succeeded);
        Assert.True(await _fixture.UserManager.IsInRoleAsync(user, "Member"));
        Assert.Contains(
            await _fixture.UserManager.GetLoginsAsync(user),
            login => login.LoginProvider == Provider);
    }

    [Fact]
    public async Task CreateWithPasswordAsync_refuse_un_mot_de_passe_trop_faible()
    {
        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());
        var user = new ApplicationUser("faible@example.org");

        var resultat = await service.CreateWithPasswordAsync(
            user,
            "court",
            Provider,
            ProviderKey,
            "Google");

        Assert.False(resultat.Succeeded);
        Assert.Null(await _fixture.UserManager.FindByEmailAsync("faible@example.org"));
    }

    [Fact]
    public async Task CreateWithPasswordAsync_supprime_le_compte_si_le_rattachement_echoue()
    {
        // La clé d'identité est déjà prise par un autre compte : le rattachement du nouveau
        // compte échouera, et c'est précisément le cas qui laissait un compte orphelin
        // occupant l'adresse.
        var premier = new ApplicationUser("premier@example.org");
        await _fixture.UserManager.CreateAsync(premier, "MotDePasseSolide1!");
        await _fixture.UserManager.AddLoginAsync(
            premier,
            new UserLoginInfo(Provider, ProviderKey, "Google"));

        var service = _fixture.CreateAccountService(IdentityFixture.CreateAnonymousAccessor());

        var resultat = await service.CreateWithPasswordAsync(
            new ApplicationUser("second@example.org"),
            "MotDePasseSolide1!",
            Provider,
            ProviderKey,
            "Google");

        Assert.False(resultat.Succeeded);

        // Le compte à moitié créé ne doit pas subsister : l'adresse resterait occupée et le
        // membre ne pourrait plus recommencer sa saisie.
        Assert.Null(await _fixture.UserManager.FindByEmailAsync("second@example.org"));

        // Le compte d'origine, lui, est intact.
        Assert.NotNull(await _fixture.UserManager.FindByEmailAsync("premier@example.org"));
    }
}
