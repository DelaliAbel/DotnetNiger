using System.Net;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Compose la redirection de réponse du parcours « mot de passe oublié ».
/// </summary>
/// <remarks>
/// Cette redirection est la seule réponse que reçoit la personne qui demande un lien. Elle est
/// calculée à partir de l'adresse saisie et de la configuration du serveur, jamais de l'existence
/// d'un compte : ajouter ici un paramètre lié au compte — « inconnu », « échec de l'envoi » —
/// transformerait le formulaire en annuaire des membres inscrits, exploitable par n'importe qui
/// avec une liste d'adresses. Un échec d'envoi réel est un incident d'exploitation : il se
/// signale dans le journal (événement 49) et par la supervision, pas dans la réponse.
/// </remarks>
public static class PasswordResetRedirect
{
    /// <summary>
    /// Construit la redirection de confirmation.
    /// </summary>
    /// <param name="email">Adresse saisie, renvoyée pour rappeler au membre où chercher.</param>
    /// <param name="emailConfigure">
    /// Vrai si l'instance dispose d'un serveur de courriels. L'avertissement affiché alors est
    /// une propriété de l'instance, identique quel que soit le compte demandé.
    /// </param>
    /// <returns>Redirection vers la page « mot de passe oublié ».</returns>
    public static string Confirmation(string email, bool emailConfigure)
    {
        var cible = $"/compte/mot-de-passe-oublie?envoye=1&courriel={WebUtility.UrlEncode(email)}";

        return emailConfigure
            ? cible
            : cible + "&courrielNonConfigure=1";
    }
}
