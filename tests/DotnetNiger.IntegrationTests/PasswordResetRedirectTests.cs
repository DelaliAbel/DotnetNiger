using DotnetNiger.Web.Authentication;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie que la réponse du formulaire « mot de passe oublié » ne révèle pas les comptes.
/// </summary>
/// <remarks>
/// Ce formulaire est ouvert à tous et n'exige aucun jeton d'invitation : il ne peut donc produire
/// qu'une seule réponse pour une adresse donnée, quelle que soit l'inscription. Toute variante
/// ajoutée par la suite — « compte inconnu », « envoi impossible » — allowait de balayer une liste
/// d'adresses et de constituer l'annuaire des membres du site, ce que le formulaire
/// d'inscription ne permet pas de façon exploitable.
/// </remarks>
public class PasswordResetRedirectTests
{
    [Fact]
    public void La_reponse_ne_dépend_que_de_l_adresse_saisie()
    {
        var url = PasswordResetRedirect.Confirmation("amina@example.org", emailConfigure: true);

        Assert.Equal(
            "/compte/mot-de-passe-oublie?envoye=1&courriel=amina%40example.org",
            url);
    }

    [Fact]
    public void Aucun_parametre_ne_signale_le_compte_ni_la_reussite_de_l_envoi()
    {
        var url = PasswordResetRedirect.Confirmation("amina@example.org", emailConfigure: true);

        // La seule information tenant lieu de compte est l'adresse que le demandeur connaît déjà.
        // Ni « inconnu », ni « échec de l'envoi », ni compteur : la réponse est constante.
        var parametres = url.Split('?', 2)[1]
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(parametre => parametre.Split('=', 2)[0])
            .ToArray();

        Assert.Equal(["envoye", "courriel"], parametres);
    }

    [Fact]
    public void L_avertissement_de_configuration_vient_du_serveur_et_non_du_compte()
    {
        // L'instance sans serveur de courriels doit le dire pour toute adresse, inscrite ou non :
        // sinon l'absence de l'avertissement devient elle-même un signal d'inscription.
        var configure = PasswordResetRedirect.Confirmation("amina@example.org", emailConfigure: true);
        var nonConfigure = PasswordResetRedirect.Confirmation("amina@example.org", emailConfigure: false);

        Assert.Equal(configure + "&courrielNonConfigure=1", nonConfigure);
    }

    [Fact]
    public void L_adresse_est_encodée_pour_ne_pas_trahir_les_parametres_de_l_url()
    {
        var url = PasswordResetRedirect.Confirmation("a+tag@example.org&erreur=1", emailConfigure: false);

        Assert.DoesNotContain("erreur=1", url, StringComparison.Ordinal);
        Assert.Contains("courrielNonConfigure=1", url, StringComparison.Ordinal);
    }
}
