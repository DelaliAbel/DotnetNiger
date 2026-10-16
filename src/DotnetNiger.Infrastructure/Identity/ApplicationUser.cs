using DotnetNiger.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Membre de la communauté, identifié par un compte ASP.NET Identity.
/// </summary>
/// <remarks>
/// Volontairement placée dans Infrastructure et non dans le domaine : l'héritage
/// d'<see cref="IdentityUser{TKey}"/> arrastre les abstractions de persistance
/// d'Identity, ce qui contrevient à la pureté du domaine. Les entités du domaine la
/// référencent par son identifiant, jamais par navigation.
/// </remarks>
public sealed class ApplicationUser : IdentityUser<Guid>, IEntity, ITimestamped
{
    /// <summary>
    /// Initialise un utilisateur sans nom, réservé à Entity Framework Core.
    /// </summary>
    public ApplicationUser()
    {
        IsActive = true;
    }

    /// <summary>
    /// Initialise un utilisateur avec son adresse de courriel, qui sert aussi de nom d'utilisateur.
    /// </summary>
    /// <param name="email">Adresse de courriel, utilisée comme identifiant de connexion.</param>
    /// <remarks>
    /// Les champs normalisés (<c>NormalizedEmail</c>, <c>NormalizedUserName</c>) sont
    /// laissés vides : <c>UserManager</c> les renseigne à la création. Les recalculer ici
    /// dupliquerait sa logique et risquerait de diverger.
    /// </remarks>
    public ApplicationUser(string email)
        : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email.Trim();
        UserName = email.Trim();
    }

    /// <summary>
    /// Obtient le prénom.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le nom.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug d'URL du profil, renseigné au premier enregistrement complet.
    /// </summary>
    public string? Slug { get; private set; }

    /// <summary>
    /// Obtient la biographie publique.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Obtient l'URL de la photo de profil, stockée en base.
    /// </summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>
    /// Obtient la ville de résidence.
    /// </summary>
    public string? City { get; private set; }

    /// <summary>
    /// Obtient la fonction professionnelle déclarée par le membre.
    /// </summary>
    /// <remarks>
    /// Texte libre et non liste : les professions de la communauté ne se laissent pas
    /// énumérer. Le rôle technique — administrateur ou membre — reste un rôle ASP.NET,
    /// distinct de ce que le membre exerce dans la vie professionnelle.
    /// </remarks>
    public string? Profession { get; private set; }

    /// <summary>
    /// Obtient l'identifiant GitHub, utilisé pour le rapprochement automatique des contributeurs.
    /// </summary>
    public string? GitHubUsername { get; private set; }

    /// <summary>
    /// Obtient l'URL du profil LinkedIn.
    /// </summary>
    public string? LinkedInUrl { get; private set; }

    /// <summary>
    /// Obtient l'URL du profil X.
    /// </summary>
    public string? XUrl { get; private set; }

    /// <summary>
    /// Obtient la liste des compétences et technologies, séparées par des virgules.
    /// </summary>
    public string? Technologies { get; private set; }

    /// <summary>
    /// Obtient une valeur indiquant si le membre fait partie de l'équipe organisatrice.
    /// </summary>
    /// <remarks>Booléen et non rôle, conformément à la décision sur les rôles.</remarks>
    public bool IsTeam { get; private set; }

    /// <summary>
    /// Obtient une valeur indiquant si le compte est actif. Un compte désactivé ne peut plus se connecter.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <inheritdoc />
    public DateTime CreatedAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Obtient le nom complet, utilisé dans les listes et les commentaires.
    /// </summary>
    public string FullName
    {
        get
        {
            var full = $"{FirstName} {LastName}".Trim();
            return full.Length > 0 ? full : (Email ?? UserName ?? string.Empty);
        }
    }

    /// <summary>
    /// Met à jour le profil public du membre.
    /// </summary>
    /// <param name="firstName">Prénom.</param>
    /// <param name="lastName">Nom.</param>
    /// <param name="bio">Biographie, facultative.</param>
    /// <param name="city">Ville, facultative.</param>
    /// <param name="photoUrl">URL de la photo, facultative.</param>
    /// <param name="gitHubUsername">Identifiant GitHub, facultatif.</param>
    /// <param name="linkedInUrl">URL LinkedIn, facultative.</param>
    /// <param name="xUrl">URL X, facultative.</param>
    /// <param name="technologies">Compétences, facultatives.</param>
    /// <param name="profession">Fonction professionnelle, facultative.</param>
    public void UpdateProfile(
        string firstName,
        string lastName,
        string? bio = null,
        string? city = null,
        string? photoUrl = null,
        string? gitHubUsername = null,
        string? linkedInUrl = null,
        string? xUrl = null,
        string? technologies = null,
        string? profession = null)
    {
        FirstName = Required(firstName, FieldLimits.Name, nameof(firstName));
        LastName = Required(lastName, FieldLimits.Name, nameof(lastName));
        Bio = DomainGuard.OptionalAgainstTooLong(bio, FieldLimits.Bio, nameof(bio));
        City = DomainGuard.OptionalAgainstTooLong(city, FieldLimits.Location, nameof(city));
        Profession = DomainGuard.OptionalAgainstTooLong(profession, FieldLimits.Label, nameof(profession));
        PhotoUrl = DomainGuard.OptionalAgainstImageSource(photoUrl, nameof(photoUrl));
        GitHubUsername = NormalizeHandle(gitHubUsername, nameof(gitHubUsername));
        LinkedInUrl = DomainGuard.OptionalAgainstHttpUrl(linkedInUrl, nameof(linkedInUrl));
        XUrl = DomainGuard.OptionalAgainstHttpUrl(xUrl, nameof(xUrl));
        Technologies = DomainGuard.OptionalAgainstTooLong(technologies, FieldLimits.Technologies, nameof(technologies));
        Slug ??= SlugFactory.CreateWithSuffix($"{FirstName}-{LastName}", Id);
    }

    /// <summary>
    /// Définit l'appartenance à l'équipe organisatrice.
    /// </summary>
    /// <param name="isTeam"><see langword="true"/> pour afficher le membre dans l'équipe.</param>
    public void SetTeamMember(bool isTeam) => IsTeam = isTeam;

    /// <summary>
    /// Active ou désactive le compte. Un compte désactivé ne peut plus se connecter.
    /// </summary>
    /// <param name="isActive"><see langword="true"/> pour réactiver le compte.</param>
    public void SetActive(bool isActive) => IsActive = isActive;

    /// <summary>
    /// Définit la photo de profil, sans toucher aux autres champs.
    /// </summary>
    /// <remarks>
    /// Une méthode dédiée plutôt que <see cref="UpdateProfile"/> : à l'inscription par
    /// fournisseur externe, le prénom et le nom sont encore vides, et les revalider à cet
    /// endroit ferait échouer la photo sur un compte parfaitement valide.
    /// </remarks>
    /// <param name="photoUrl">Adresse de la photo, ou <see langword="null"/> pour la retirer.</param>
    /// <exception cref="DotnetNiger.Domain.Exceptions.DomainException">
    /// L'adresse n'est ni une URL HTTP ou HTTPS, ni un chemin servi par le site.
    /// </exception>
    /// <remarks>
    /// La photo est presque toujours déposée par l'administration, donc servie par le site
    /// lui-même à une adresse de la forme <c>/api/images/…</c>. N'accepter qu'une URL absolue
    /// obligerait à écrire le nom de domaine dans chaque profil, et ferait échouer la photo
    /// d'un membre si le site changeait d'adresse.
    /// </remarks>
    public void SetProfilePhoto(string? photoUrl) =>
        PhotoUrl = DomainGuard.OptionalAgainstImageSource(photoUrl, nameof(photoUrl));

    private static string? NormalizeHandle(string? value, string field)
    {
        var handle = DomainGuard.OptionalAgainstTooLong(value, FieldLimits.Handle, field);

        return string.IsNullOrWhiteSpace(handle) ? null : handle.TrimStart('@');
    }

    private static string Required(string value, int maxLength, string field)
        => DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(value, field),
            maxLength,
            field);
}
