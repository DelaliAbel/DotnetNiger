namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un article de blog ou de publication.
/// </summary>
public class Post
{
    /// <summary>Identifiant unique de la publication.</summary>
    public Guid Id { get; set; }
    /// <summary>Titre de la publication.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Contenu de la publication (Markdown ou HTML).</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Résumé de la publication.</summary>
    public string Excerpt { get; set; } = string.Empty;
    /// <summary>URL de l'image de couverture.</summary>
    public string CoverImageUrl { get; set; } = string.Empty;
    /// <summary>Identifiant de l'auteur.</summary>
    public Guid AuthorId { get; set; }
    /// <summary>Nom de l'auteur.</summary>
    public string AuthorName { get; set; } = string.Empty;
    /// <summary>Avatar de l'auteur.</summary>
    public string AuthorAvatar { get; set; } = string.Empty;
    /// <summary>Type d'article (article, tutoriel, actualité, etc.).</summary>
    public string PostType { get; set; } = string.Empty;
    /// <summary>Statut de publication.</summary>
    public PostStatus Status { get; set; } = PostStatus.Draft;
    /// <summary>Indique si la publication est publiée.</summary>
    public bool IsPublished { get; set; }
    /// <summary>Indique si la publication est supprimée logiquement.</summary>
    public bool IsDeleted { get; set; }
    /// <summary>Nombre de vues.</summary>
    public int ViewCount { get; set; }
    /// <summary>Date de publication.</summary>
    public DateTime? PublishedAt { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation vers l'auteur.</summary>
    public ApplicationUser? Author { get; set; }
    /// <summary>Catégories de la publication.</summary>
    public ICollection<PostCategory> PostCategories { get; set; } = [];
    /// <summary>Tags de la publication.</summary>
    public ICollection<PostTag> PostTags { get; set; } = [];
    /// <summary>Commentaires de la publication.</summary>
    public ICollection<Comment> Comments { get; set; } = [];
}

public enum PostStatus
{
    Draft,
    PendingReview,
    Published,
    Archived
}
