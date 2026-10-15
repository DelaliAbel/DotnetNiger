using DotnetNiger.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller.Common;

/// <summary>
/// Socle commun des contrôleurs qui traitent les envois de formulaires SSR.
/// </summary>
/// <remarks>
/// Ces contrôleurs complètent l'API JSON : ils reçoivent un formulaire HTML publié par une
/// page Blazor et renvoient une redirection, pas du JSON. Ils se distinguent donc de
/// <see cref="ApiControllerBase"/> sur deux points — pas de préfixe <c>/api</c>, pas de
/// <c>[IgnoreAntiforgeryToken]</c>. Le jeton reste exigé, parce qu'un formulaire navigue
/// réellement : c'est la requête intersite qui doit être bloquée ici, alors que l'API s'en
/// protège par un cookie <c>SameSite=Lax</c>.
/// </remarks>
public abstract class FormulaireControllerBase : Microsoft.AspNetCore.Mvc.Controller
{
    /// <summary>
    /// Indique si la demande vient d'un membre authentifié.
    /// </summary>
    /// <param name="utilisateur">Service d'identité de la requête.</param>
    /// <returns><see langword="true"/> si un membre connecté a été reconnu.</returns>
    protected static bool EstMembre(ICurrentUserService utilisateur)
        => utilisateur.IsAuthenticated
            && utilisateur.UserId is { } identifiant
            && identifiant != Guid.Empty;

    /// <summary>
    /// Indique si la demande vient d'un membre authentifié, en recoupant le service
    /// d'identité avec l'identité de la requête.
    /// </summary>
    /// <param name="contexte">Requête en cours.</param>
    /// <param name="utilisateur">Service d'identité de la requête.</param>
    /// <returns><see langword="true"/> si un membre connecté a été reconnu des deux côtés.</returns>
    /// <remarks>
    /// Recouper les deux sources évite qu'une écriture ne parte sur une simple clé d'utilisateur
    /// alors que le pipeline d'authentification n'a rien reconnu.
    /// </remarks>
    protected static bool EstMembre(HttpContext contexte, ICurrentUserService utilisateur)
        => contexte.User.Identity?.IsAuthenticated == true && EstMembre(utilisateur);

    /// <summary>
    /// Ne conserve qu'un chemin interne, comme le fait le formulaire d'abonnement.
    /// </summary>
    /// <param name="chemin">Chemin transmis par le navigateur.</param>
    /// <returns>Le chemin sûr vers lequel renvoyer.</returns>
    /// <remarks>
    /// Un chemin fourni par le client ne doit pas pouvoir transformer une redirection en
    /// ouverture vers un autre site.
    /// </remarks>
    protected static string CheminAutorise(string? chemin)
    {
        if (string.IsNullOrWhiteSpace(chemin)
            || !chemin.StartsWith('/')
            || chemin.StartsWith("//", StringComparison.Ordinal))
        {
            return "/blog";
        }

        return chemin;
    }

    /// <summary>
    /// Renvoie le visiteur vers la connexion en gardant le chemin d'arrivée.
    /// </summary>
    /// <param name="chemin">Chemin vers lequel revenir une fois connecté.</param>
    /// <returns>Redirection vers la page de connexion.</returns>
    protected IActionResult RedirectionConnexion(string chemin)
        => Redirect($"/compte/connexion?retour={Uri.EscapeDataString(chemin)}");

    /// <summary>
    /// Renvoie le visiteur vers une page en lui passant un message par l'URL.
    /// </summary>
    /// <param name="chemin">Page d'arrivée.</param>
    /// <param name="cle">Nom du paramètre, <c>confirmation</c> ou <c>erreur</c>.</param>
    /// <param name="message">Message affiché à l'utilisateur.</param>
    /// <returns>Redirection vers la page d'arrivée.</returns>
    /// <remarks>
    /// Le message revient par l'URL : une écriture refusée doit pouvoir l'expliquer sans que la
    /// page perde le reste de son contenu.
    /// </remarks>
    protected IActionResult RedirectionAvecMessage(string chemin, string cle, string message)
        => Redirect($"{chemin}?{cle}={Uri.EscapeDataString(message)}");
}