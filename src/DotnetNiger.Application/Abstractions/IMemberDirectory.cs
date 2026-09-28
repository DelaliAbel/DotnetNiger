namespace DotnetNiger.Application.Abstractions;

/// <summary>
/// Résout l'adresse de courriel d'un membre, pour les messages qui lui sont destinés.
/// </summary>
/// <remarks>
/// Les courriels sortants ne sont pas tous destinés à l'appelant connecté : une inscription
/// à un événement, un accusé de réception, une promotion depuis une liste d'attente
/// concernent tous quelqu'un d'autre. Or l'utilisateur courant ne connaît que son propre
/// identifiant, et l'entité d'identité ne fait pas partie du domaine.
/// <para>
/// Cette interface est donc le seul passage autorisé entre les deux mondes. Son
/// implémentation peut lire la table des comptes sans que le domaine en sache rien, et un
/// test peut en fournir une doublure sans monter de base de données.
/// </para>
/// </remarks>
public interface IMemberDirectory
{
    /// <summary>
    /// Rend les coordonnées d'un membre.
    /// </summary>
    /// <param name="memberId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les coordonnées, ou <see langword="null"/> si le membre est introuvable.</returns>
    /// <remarks>
    /// Renvoyer <see langword="null"/> plutôt qu'une exception est délibéré : un compte sans
    /// adresse ne doit pas faire échouer une inscription à un événement. L'appelant décide
    /// ce qu'il fait de l'absence, le plus souvent il renonce à l'envoi et le consigne.
    /// </remarks>
    Task<MemberContact?> ObtenirAsync(Guid memberId, CancellationToken cancellationToken);
}

/// <summary>
/// Coordonnées d'un membre, telles qu'un courriel les utilise.
/// </summary>
/// <param name="Email">Adresse de courriel, éventuellement vide.</param>
/// <param name="FullName">Nom affiché, à défaut l'adresse elle-même.</param>
public sealed record MemberContact(string Email, string FullName);
