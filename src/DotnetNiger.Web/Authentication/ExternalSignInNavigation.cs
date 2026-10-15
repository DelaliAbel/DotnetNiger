using Microsoft.AspNetCore.Authentication;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Fonctions de navigation du parcours de connexion externe.
/// </summary>
public static class ExternalSignInNavigation
{
    /// <summary>
    /// Clé sous laquelle la destination demandée voyage dans les propriétés
    /// d'authentification.
    /// </summary>
    public const string ReturnUrlItem = "returnUrl";

    /// <summary>
    /// Prépare les propriétés du défi envoyé au fournisseur.
    /// </summary>
    /// <param name="returnUrl">Destination souhaitée après la connexion.</param>
    /// <param name="callbackPath">Adresse de retour déclarée chez le fournisseur.</param>
    /// <returns>Propriétés du défi.</returns>
    /// <remarks>
    /// Le fournisseur attend une réponse d'authentification, pas une redirection : renvoyer
    /// une simple redirection vers la page de départ ne déclencherait jamais la fenêtre de
    /// connexion chez lui. La destination demandée est conservée ici parce qu'elle ne peut
    /// plus transiter par l'URL du retour, interceptée par le fournisseur.
    /// </remarks>
    public static AuthenticationProperties BuildChallengeProperties(
        string? returnUrl,
        string callbackPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callbackPath);

        return new AuthenticationProperties
        {
            RedirectUri = callbackPath,

            // Une destination qui ne reste pas dans le site est ramenée à l'espace du
            // membre, plutôt que d'être transmise au fournisseur puis restituée telle
            // quelle : se connecter avec un compte externe doit mener au même endroit
            // qu'une connexion par mot de passe.
            Items = { [ReturnUrlItem] = IsLocal(returnUrl) ? returnUrl! : EspaceMembre },
        };
    }

    /// <summary>
    /// Destination retenue quand la connexion n'en précise aucune : l'espace du membre.
    /// </summary>
    /// <remarks>
    /// Partagée par la connexion locale et la connexion par fournisseur, pour que les deux
    /// parcours ne proposeront jamais deux destinations différentes.
    /// </remarks>
    public const string EspaceMembre = "/compte/tableau-de-bord";

    /// <summary>
    /// Relit la destination demandée dans les propriétés d'authentification.
    /// </summary>
    /// <param name="properties">Propriétés reçues au retour du fournisseur.</param>
    /// <returns>Destination interne, ou null si elle est absente ou externe.</returns>
    public static string? ReadReturnUrl(AuthenticationProperties? properties)
    {
        if (properties is null)
        {
            return null;
        }

        return properties.Items.TryGetValue(ReturnUrlItem, out var valeur) && IsLocal(valeur)
            ? valeur
            : null;
    }

    /// <summary>
    /// Vrai si une cible de redirection reste dans le site.
    /// </summary>
    /// <param name="url">Cible à vérifier.</param>
    /// <returns>Vrai si la cible est locale.</returns>
    /// <remarks>
    /// Seule une barre oblique initiale est acceptée. Une URL absolue, même vers le même
    /// domaine, permettrait de fabriquer un lien qui expédie le membre ailleurs après sa
    /// connexion.
    /// </remarks>
    public static bool IsLocal(string? url) =>
        !string.IsNullOrEmpty(url)
        && url[0] == '/'
        && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
}
