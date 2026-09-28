using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Administration;

/// <summary>
/// Formulaire de rédaction d'une actualité.
/// </summary>
/// <remarks>
/// Les mêmes champs servent à la création et à la modification : un formulaire de
/// modification qui omet un champ le viderait, et l'article deviendrait muet sur un
/// simple oubli.
/// </remarks>
public sealed class ActualiteInput
{
    /// <summary>
    /// Titre de l'article.
    /// </summary>
    [Required(ErrorMessage = "Indiquez un titre.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Le titre doit comporter au moins 3 caractères.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Chapô affiché dans les listes et les partages.
    /// </summary>
    [Required(ErrorMessage = "Indiquez un chapô.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Le chapô doit comporter au moins 10 caractères.")]
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// Contenu rédigé.
    /// </summary>
    [Required(ErrorMessage = "Indiquez un contenu.")]
    [StringLength(20000, MinimumLength = 20, ErrorMessage = "Le contenu doit comporter au moins 20 caractères.")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Catégorie de l'article.
    /// </summary>
    [EnumDataType(typeof(ArticleCategory), ErrorMessage = "Choisissez une catégorie.")]
    public ArticleCategory Category { get; set; } = ArticleCategory.DotNetNews;

    /// <summary>
    /// Image de couverture.
    /// </summary>
    /// <remarks>
    /// La validation est faite par <see cref="ActualiteInput.Validate"/> plutôt que par
    /// <see cref="UrlAttribute"/> : l'annotation refuse une chaîne vide, ce qui
    /// interdirait d'enregistrer une actualité sans image — le cas le plus courant.
    /// </remarks>
    [StringLength(500)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Publier aussitôt, plutôt que garder en brouillon.
    /// </summary>
    public bool Publish { get; set; }

    /// <summary>
    /// Vérifie l'adresse d'image, qui peut être absente.
    /// </summary>
    /// <param name="validationContext">Contexte de validation, non utilisé.</param>
    /// <returns>Les erreurs trouvées, aucune si l'adresse est valide ou absente.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(CoverImageUrl))
        {
            yield break;
        }

        var estValide = Uri.TryCreate(CoverImageUrl, UriKind.Absolute, out var adresse)
            && (string.Equals(adresse.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(adresse.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));

        if (!estValide)
        {
            yield return new ValidationResult(
                "Cette adresse d'image n'est pas valide.",
                [nameof(CoverImageUrl)]);
        }
    }
}
