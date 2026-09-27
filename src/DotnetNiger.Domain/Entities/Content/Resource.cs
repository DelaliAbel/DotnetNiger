using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
/// Ressource proposée à la communauté : formation, tutoriel, guide, bibliothèque ou outil.
/// </summary>
/// <remarks>
/// Contrairement à l'article, une ressource peut être publiée par n'importe quel membre
/// connecté, sans validation préalable. Elle pointe soit vers une ressource externe
/// (Microsoft Learn, documentation officielle), soit vers un contenu rédigé localement.
/// </remarks>
public sealed class Resource : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Resource()
    {
    }

    /// <summary>
    /// Initialise une nouvelle ressource à l'état de brouillon.
    /// </summary>
    /// <param name="creatorId">Identifiant du membre contributeur.</param>
    /// <param name="title">Titre de la ressource.</param>
    /// <param name="description">Description courte.</param>
    /// <param name="category">Catégorie de ressource.</param>
    public Resource(Guid creatorId, string title, string description, ResourceCategory category)
    {
        CreatorId = DomainGuard.AgainstEmpty(creatorId, nameof(creatorId));
        Title = Required(title, FieldLimits.Title, nameof(title));
        Description = Required(description, FieldLimits.Excerpt, nameof(description));
        Category = category;
        Slug = SlugFactory.Create(title);
    }

    /// <summary>
    /// Obtient le titre de la ressource.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug d'URL interne, unique dans la base.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la description courte.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le contenu rédigé localement, si la ressource n'est pas uniquement un lien.
    /// </summary>
    public string? Content { get; private set; }

    /// <summary>
    /// Obtient l'image de couverture, utilisée pour la vignette de la ressource.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Obtient l'URL de la ressource externe, si elle existe.
    /// </summary>
    public string? ExternalUrl { get; private set; }

    /// <summary>
    /// Obtient l'éditeur ou la source de la ressource, par exemple « Microsoft Learn ».
    /// </summary>
    public string? Provider { get; private set; }

    /// <summary>
    /// Obtient la catégorie de ressource.
    /// </summary>
    public ResourceCategory Category { get; private set; }

    /// <summary>
    /// Obtient l'état éditorial.
    /// </summary>
    public PublicationStatus Status { get; private set; }

    /// <summary>
    /// Obtient la date de publication (UTC), renseignée à la première publication.
    /// </summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du membre contributeur.
    /// </summary>
    public Guid CreatorId { get; private set; }

    /// <summary>
    /// Met à jour les informations descriptives de la ressource.
    /// </summary>
    /// <param name="title">Nouveau titre.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <param name="category">Nouvelle catégorie.</param>
    public void UpdateDetails(string title, string description, ResourceCategory category)
    {
        var newTitle = Required(title, FieldLimits.Title, nameof(title));

        if (!string.Equals(Title, newTitle, StringComparison.Ordinal))
        {
            Title = newTitle;
            Slug = SlugFactory.Create(newTitle);
        }

        Description = Required(description, FieldLimits.Excerpt, nameof(description));
        Category = category;
    }

    /// <summary>
    /// Rattache la ressource à un contenu externe.
    /// </summary>
    /// <param name="externalUrl">URL absolue de la ressource.</param>
    /// <param name="provider">Éditeur ou source, facultatif.</param>
    public void LinkTo(string externalUrl, string? provider = null)
    {
        ExternalUrl = DomainGuard.AgainstHttpUrl(externalUrl, nameof(externalUrl));
        Provider = DomainGuard.OptionalAgainstTooLong(provider, FieldLimits.Name, nameof(provider));
    }

    /// <summary>
    /// Remplace l'image de couverture de la ressource.
    /// </summary>
    /// <param name="coverImageUrl">URL absolue, ou <see langword="null"/> pour retirer l'image.</param>
    /// <remarks>
    /// L'image sert à la vignette de la ressource dans les listes. Elle est facultative :
    /// une ressource partagée sous forme de lien externe n'a pas toujours d'image associée.
    /// </remarks>
    public void SetCoverImage(string? coverImageUrl)
        => CoverImageUrl = DomainGuard.OptionalAgainstHttpUrl(coverImageUrl, nameof(coverImageUrl));

    /// <summary>
    /// Définit le contenu rédigé localement.
    /// </summary>
    /// <param name="content">Contenu au format HTML, ou <see langword="null"/> pour le retirer.</param>
    public void SetContent(string? content)
    {
        var value = DomainGuard.OptionalAgainstTooLong(content, FieldLimits.Content, nameof(content));
        Content = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Publie la ressource.
    /// </summary>
    /// <param name="nowUtc">Date de publication (UTC).</param>
    public void Publish(DateTime nowUtc)
    {
        Status = PublicationStatus.Published;
        PublishedAtUtc ??= nowUtc;
    }

    /// <summary>
    /// Archive la ressource.
    /// </summary>
    public void Archive() => Status = PublicationStatus.Archived;

    private static string Required(string value, int maxLength, string field)
        => DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(value, field),
            maxLength,
            field);
}
