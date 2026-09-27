using System.Globalization;
using System.Text;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Common;

/// <summary>
/// Fabrique de slugs URL pour les pages publiques.
/// </summary>
public static class SlugFactory
{
    /// <summary>
    /// Longueur maximale d'un slug.
    /// </summary>
    public const int MaxLength = FieldLimits.Slug;

    /// <summary>
    /// Transforme un texte libre en slug URL : minuscules, séparateurs tirets, sans accent.
    /// </summary>
    /// <param name="value">Texte source.</param>
    /// <returns>Slug normalisé.</returns>
    /// <exception cref="DomainException">Le texte ne produit aucun caractère utilisable.</exception>
    public static string Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("invalid_slug", "Le texte fourni ne produit aucun slug valide.");
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiDigit(character) || char.IsAsciiLetterLower(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
            }
            else if (char.IsAsciiLetterUpper(character))
            {
                builder.Append(char.ToLower(character, CultureInfo.InvariantCulture));
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length == 0)
        {
            throw new DomainException("invalid_slug", "Le texte fourni ne produit aucun slug valide.");
        }

        return slug.Length <= MaxLength ? slug : slug[..MaxLength].TrimEnd('-');
    }

    /// <summary>
    /// Génère un slug unique en suffixant le slug de base par un identifiant court.
    /// </summary>
    /// <param name="value">Texte source.</param>
    /// <param name="discriminator">Élément assurant l'unicité, typiquement l'identifiant de l'entité.</param>
    /// <returns>Slug normalisé et suffixé.</returns>
    public static string CreateWithSuffix(string value, Guid discriminator)
    {
        var slug = Create(value);
        var suffix = discriminator.ToString("N", CultureInfo.InvariantCulture)[..8];

        return $"{slug[..Math.Min(slug.Length, MaxLength - 9)]}-{suffix}";
    }
}
