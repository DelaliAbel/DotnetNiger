using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Controller.Common;

/// <summary>
/// Gardes d'autorisation partagées par les contrôleurs.
/// </summary>
/// <remarks>
/// Une garde lève une <see cref="DomainException"/> plutôt que de renvoyer un résultat :
/// le traitement global la traduit en réponse HTTP, et le contrôleur n'a pas à répéter le
/// test de rôle à chaque action.
/// </remarks>
public abstract partial class ApiControllerBase
{
    /// <summary>
    /// Exige un utilisateur authentifié.
    /// </summary>
    /// <param name="currentUser">Service utilisateur courant.</param>
    /// <returns>Identifiant de l'utilisateur.</returns>
    /// <exception cref="DomainException">Si la requête est anonyme.</exception>
    /// 
    /// 
    /// 
    protected static Guid RequireUser(ICurrentUserService currentUser) =>
        currentUser.UserId
        ?? throw new DomainException("forbidden", "Connectez-vous pour effectuer cette action.");

    /// <summary>
    /// Exige que l'appelant soit l'auteur de la ressource ou un administrateur.
    /// </summary>
    /// <param name="currentUser">Service utilisateur courant.</param>
    /// <param name="auteurId">Identifiant de l'auteur de la ressource.</param>
    /// <returns>Identifiant de l'appelant.</returns>
    /// <exception cref="DomainException">Si la requête est anonyme ou si l'appelant n'est ni l'auteur ni un administrateur.</exception>
    /// <remarks>
    /// Un membre peut gérer ce qu'il a publié, mais pas le contenu des autres : sans cette
    /// garde, il suffirait de deviner un identifiant pour modifier l'article d'autrui.
    /// L'administrateur, lui, intervient partout, y compris pour la modération.
    /// </remarks>
    protected static Guid RequireAuteurOuAdministrateur(ICurrentUserService currentUser, Guid auteurId)
    {
        var utilisateur = RequireUser(currentUser);

        if (utilisateur != auteurId && !currentUser.IsInRole(RoleNames.Admin))
        {
            throw new DomainException(
                "forbidden",
                "Seul l'auteur de ce contenu ou un administrateur peut le modifier.");
        }

        return utilisateur;
    }

    /// <summary>
    /// Exige le rôle d'administrateur.
    /// </summary>
    /// <param name="currentUser">Service utilisateur courant.</param>
    /// <returns>Identifiant de l'administrateur.</returns>
    /// <exception cref="DomainException">Si l'appelant n'est pas administrateur.</exception>
    protected static Guid RequireAdministrateur(ICurrentUserService currentUser)
    {
        var utilisateur = RequireUser(currentUser);

        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new DomainException("forbidden", "Cette action est réservée aux administrateurs.");
        }

        return utilisateur;
    }
}