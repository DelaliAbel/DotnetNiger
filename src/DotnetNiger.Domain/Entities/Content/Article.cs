using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
    /// Article de blog : annonce de version de .NET, mise à jour de projet local ou interview.
    /// </summary>
    /// <remarks>
    /// Réservée à la rédaction par les administrateurs. Le contenu est produit par l'éditeur
    /// TinyMCE, stocké en HTML, et assaini avant affichage par <c>ContenuHtml.Nettoyer</c> :
    /// la neutralité ne dépend pas d'un dossier de conversion, mais d'un retrait explicite
    /// des éléments d'exécution à la lecture.
    /// </remarks>
    public sealed class Article : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Article()
    {
    }

    /// <summary>
    /// Initialise un nouvel article à l'état de brouillon.
    /// </summary>
    /// <param name="authorId">Identifiant du membre auteur, obligatoirement un administrateur.</param>
    /// <param name="title">Titre de l'article.</param>
    /// <param name="excerpt">Chapo affiché dans les listes.</param>
    /// <param name="html">Corps de l'article, produit par l'éditeur HTML.</param>
    /// <param name="rubrique">Rubrique éditoriale.</param>
    /// <param name="slug">
    /// Slug d'URL, à fournir lorsque le titre ne se prête pas à une dérivation
    /// automatique ; absent, il est déduit du titre.
    /// </param>
    public Article(Guid authorId, string title, string excerpt, string html, Rubrique rubrique, string? slug = null)
    {
        AuthorId = DomainGuard.AgainstEmpty(authorId, nameof(authorId));
        Title = Required(title, FieldLimits.Title, nameof(title));
        Excerpt = Required(excerpt, FieldLimits.Excerpt, nameof(excerpt));
        Content = Required(ArticleContent.DepuisEditeur(html), FieldLimits.Content, nameof(html));
        Rubrique = DomainGuard.AgainstNull(rubrique, nameof(rubrique));
        RubriqueId = rubrique.Id;
        Slug = string.IsNullOrWhiteSpace(slug)
            ? SlugFactory.Create(Title)
            : DomainGuard.AgainstTooLong(slug, FieldLimits.Slug, nameof(slug));
    }

    /// <summary>
    /// Obtient le titre de l'article.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug d'URL, unique dans la base.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le chapo utilisé dans les listes et le partage sur les réseaux sociaux.
    /// </summary>
    public string Excerpt { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le corps de l'article au format HTML, produit par l'éditeur.
    /// </summary>
    /// <remarks>
    /// Propriété en lecture seule : seuls le constructeur et la mise à jour l'alimentent,
    /// et tous deux passent par <see cref="ArticleContent.DepuisEditeur"/>. Un corps ne
    /// peut donc pas être écrit en balisage arbitraire sans passer par la même précaution.
    /// </remarks>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la rubrique éditoriale de l'article.
    /// </summary>
    public Rubrique Rubrique { get; private set; } = null!;

    /// <summary>
    /// Obtient l'identifiant de la rubrique, clé étrangère de la relation.
    /// </summary>
    public Guid RubriqueId { get; private set; }

    /// <summary>
    /// Obtient l'état éditorial.
    /// </summary>
    public PublicationStatus Status { get; private set; }

    /// <summary>
    /// Obtient la date de publication (UTC), renseignée à la première publication.
    /// </summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'URL de l'image de couverture, stockée en base.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du membre auteur.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// Remplace le contenu éditorial de l'article. Le slug suit uniquement le titre.
    /// </summary>
    /// <param name="title">Nouveau titre.</param>
    /// <param name="excerpt">Nouveau chapo.</param>
    /// <param name="html">Nouveau corps, produit par l'éditeur HTML.</param>
    /// <param name="rubrique">Nouvelle rubrique.</param>
    public void UpdateDetails(string title, string excerpt, string html, Rubrique rubrique)
    {
        var newTitle = Required(title, FieldLimits.Title, nameof(title));

        if (!string.Equals(Title, newTitle, StringComparison.Ordinal))
        {
            Title = newTitle;
            Slug = SlugFactory.Create(newTitle);
        }

        Excerpt = Required(excerpt, FieldLimits.Excerpt, nameof(excerpt));
        Content = Required(ArticleContent.DepuisEditeur(html), FieldLimits.Content, nameof(html));
        Rubrique = DomainGuard.AgainstNull(rubrique, nameof(rubrique));
        RubriqueId = rubrique.Id;
    }

    /// <summary>
    /// Remplace l'image de couverture.
    /// </summary>
    /// <param name="coverImageUrl">Adresse de l’image : URL absolue ou chemin du site, ou <see langword="null"/> pour la retirer.</param>
    public void SetCoverImage(string? coverImageUrl)
        => CoverImageUrl = DomainGuard.OptionalAgainstImageSource(coverImageUrl, nameof(coverImageUrl));

    /// <summary>
    /// Publie l'article. La date de publication n'est pas écrasée lors d'une republication.
    /// </summary>
    /// <param name="nowUtc">Date de publication (UTC).</param>
    public void Publish(DateTime nowUtc)
    {
        Status = PublicationStatus.Published;
        PublishedAtUtc ??= nowUtc;
    }

    /// <summary>
    /// Repasse l'article à l'état de brouillon, sans le retirer du site s'il était publié.
    /// </summary>
    public void ReturnToDraft() => Status = PublicationStatus.Draft;

    /// <summary>
    /// Archive l'article : il disparaît des listes publiques mais reste en base.
    /// </summary>
    public void Archive() => Status = PublicationStatus.Archived;

    private static string Required(string value, int maxLength, string field)
        => DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(value, field),
            maxLength,
            field);
}
