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

    /// <summary>
    /// Rend les profils publics de plusieurs membres en une seule lecture.
    /// </summary>
    /// <param name="memberIds">Identifiants des membres.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>
    /// Une correspondance identifiant vers profil, ne contenant que les membres trouvés.
    /// </returns>
    /// <remarks>
    /// Une page de six articles affiche six auteurs. Les rechercher l'un après l'autre
    /// ferait six requêtes là où une seule suffit, et le pageur d'un article s'en
    /// apercevrait immédiatement. L'absence d'un membre n'a rien d'anormal : un compte
    /// supprimé laisse ses contributions orphelines, et la page affiche alors un nom
    /// générique plutôt que de manquer d'y répondre.
    /// </remarks>
    Task<IReadOnlyDictionary<Guid, MemberProfile>> ObtenirPlusieursAsync(
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Compte les membres de la communauté.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le nombre de comptes actifs.</returns>
    /// <remarks>
    /// La désactivation est la seule façon, pour l'administration, de retirer quelqu'un du
    /// décompte : c'est donc elle qui fait la différence entre un membre et un compte laissé
    /// en attente. Le nombre est lu en base à chaque appel — l'accueil est la seule page qui
    /// l'affiche, et une valeur figée dans le code deviendrait un mensonge dès la première
    /// inscription.
    /// </remarks>
    Task<int> CompterMembresAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rend les membres de l'équipe organisatrice, tels que la page « À propos » les présente.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les membres de l'équipe, par ordre d'affichage.</returns>
    /// <remarks>
    /// L'équipe est une information de la base, portée par <c>IsTeam</c> : c'est
    /// l'administration qui la compose. La déduire des articles publiés donnerait une
    /// équipe de fait — les personnes dont on a mis en avant le texte — et le lecteur
    /// confondrait « auteur d'un article » et « membre de l'association ».
    /// </remarks>
    Task<IReadOnlyList<TeamMember>> ListerEquipeAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Membre de l'équipe organisatrice, tel que la page « À propos » le présente.
/// </summary>
/// <param name="FullName">Nom affiché.</param>
/// <param name="Role">Fonction au sein de l'équipe, facultative.</param>
/// <param name="PhotoUrl">Photo, facultative : l'absence fait tomber sur les initiales.</param>
/// <param name="MembreId">
/// Identifiant du compte, nécessaire pour retrouver la photo qu'il a déposée.
/// </param>
/// <param name="Liens">Liens que le membre a lui-même enregistrés, dans son ordre.</param>
/// <remarks>
/// Une liste, non trois colonnes. Sur son profil, le membre ajoute autant de liens qu'il
/// veut, et c'est cette liste — portfolio compris — qui est présentée sur la fiche.
/// Rendre la fiche à partir de trois champs fixes n'aurait montré qu'une adresse par
/// réseau, et aurait laissé de côté tout ce que le membre avait saisi.
/// <para>
/// Chaque lien est déjà une adresse complète, telle que le membre l'a enregistrée : la
/// fiche ne doit rien deviner sur ce qu'il a voulu y mettre.
/// </para>
/// </remarks>
public sealed record TeamMember(
    string FullName,
    string? Role,
    string? PhotoUrl,
    Guid MembreId,
    IReadOnlyList<TeamLink> Liens);

/// <summary>Lien externe présenté sur une fiche d'équipe.</summary>
/// <param name="Libelle">Libellé saisi par le membre.</param>
/// <param name="Url">Adresse de destination.</param>
public sealed record TeamLink(string Libelle, string Url);

/// <summary>
/// Profil public d'un membre, tel qu'une page de contenu l'affiche.
/// </summary>
/// <param name="FullName">Nom affiché.</param>
/// <param name="Profession">Fonction professionnelle déclarée, facultative.</param>
public sealed record MemberProfile(string FullName, string? Profession);

/// <summary>
/// Coordonnées d'un membre, telles qu'un courriel les utilise.
/// </summary>
/// <param name="Email">Adresse de courriel, éventuellement vide.</param>
/// <param name="FullName">Nom affiché, à défaut l'adresse elle-même.</param>
public sealed record MemberContact(string Email, string FullName);
