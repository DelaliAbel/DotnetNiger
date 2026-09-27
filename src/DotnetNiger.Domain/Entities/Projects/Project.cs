using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Domain.Entities.Projects;

/// <summary>
/// Projet open source porté par la communauté, listé sur la page Communauté.
/// </summary>
/// <remarks>
/// La création est réservée aux administrateurs. Les contributeurs réels sont récupérés
/// auprès de l'API GitHub puis mis en cache ; ils ne sont pas figés dans cette entité.
/// </remarks>
public sealed class Project : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Project()
    {
    }

    /// <summary>
    /// Initialise un nouveau projet à l'état de brouillon.
    /// </summary>
    /// <param name="createdById">Identifiant de l'administrateur créateur.</param>
    /// <param name="name">Nom du projet.</param>
    /// <param name="summary">Phrase d'accroche.</param>
    /// <param name="description">Description détaillée au format HTML.</param>
    /// <param name="repositoryUrl">URL du dépôt Git.</param>
    public Project(Guid createdById, string name, string summary, string description, string repositoryUrl)
    {
        CreatedById = DomainGuard.AgainstEmpty(createdById, nameof(createdById));
        Name = Required(name, FieldLimits.Name, nameof(name));
        Summary = Required(summary, FieldLimits.Excerpt, nameof(summary));
        Description = Required(description, FieldLimits.Content, nameof(description));
        RepositoryUrl = DomainGuard.AgainstHttpUrl(repositoryUrl, nameof(repositoryUrl));
        Slug = SlugFactory.Create(name);
    }

    /// <summary>
    /// Obtient le nom du projet.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug d'URL, unique dans la base.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la phrase d'accroche.
    /// </summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la description détaillée au format HTML.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'URL du dépôt Git.
    /// </summary>
    public string RepositoryUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'URL de démonstration en ligne, si elle existe.
    /// </summary>
    public string? DemoUrl { get; private set; }

    /// <summary>
    /// Obtient l'URL du logo du projet, stockée en base.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Obtient la liste des technologies utilisées, séparées par des virgules.
    /// </summary>
    public string? Technologies { get; private set; }

    /// <summary>
    /// Obtient l'état éditorial.
    /// </summary>
    public PublicationStatus Status { get; private set; }

    /// <summary>
    /// Obtient la date de publication (UTC).
    /// </summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'identifiant de l'administrateur créateur.
    /// </summary>
    public Guid CreatedById { get; private set; }

    /// <summary>
    /// Met à jour les informations descriptives du projet.
    /// </summary>
    /// <param name="name">Nouveau nom.</param>
    /// <param name="summary">Nouvelle phrase d'accroche.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <param name="repositoryUrl">Nouvelle URL de dépôt.</param>
    public void UpdateDetails(string name, string summary, string description, string repositoryUrl)
    {
        var newName = Required(name, FieldLimits.Name, nameof(name));

        if (!string.Equals(Name, newName, StringComparison.Ordinal))
        {
            Name = newName;
            Slug = SlugFactory.Create(newName);
        }

        Summary = Required(summary, FieldLimits.Excerpt, nameof(summary));
        Description = Required(description, FieldLimits.Content, nameof(description));
        RepositoryUrl = DomainGuard.AgainstHttpUrl(repositoryUrl, nameof(repositoryUrl));
    }

    /// <summary>
    /// Définit l'URL de démonstration.
    /// </summary>
    /// <param name="demoUrl">URL absolue, ou <see langword="null"/>.</param>
    public void SetDemoUrl(string? demoUrl)
        => DemoUrl = DomainGuard.OptionalAgainstHttpUrl(demoUrl, nameof(demoUrl));

    /// <summary>
    /// Définit le logo du projet.
    /// </summary>
    /// <param name="imageUrl">URL absolue, ou <see langword="null"/>.</param>
    public void SetImageUrl(string? imageUrl)
        => ImageUrl = DomainGuard.OptionalAgainstHttpUrl(imageUrl, nameof(imageUrl));

    /// <summary>
    /// Définit les technologies utilisées.
    /// </summary>
    /// <param name="technologies">Liste séparée par des virgules, ou <see langword="null"/>.</param>
    public void SetTechnologies(string? technologies)
        => Technologies = DomainGuard.OptionalAgainstTooLong(technologies, FieldLimits.Technologies, nameof(technologies));

    /// <summary>
    /// Publie le projet.
    /// </summary>
    /// <param name="nowUtc">Date de publication (UTC).</param>
    public void Publish(DateTime nowUtc)
    {
        Status = PublicationStatus.Published;
        PublishedAtUtc ??= nowUtc;
    }

    /// <summary>
    /// Archive le projet.
    /// </summary>
    public void Archive() => Status = PublicationStatus.Archived;

    private static string Required(string value, int maxLength, string field)
        => DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(value, field),
            maxLength,
            field);
}
