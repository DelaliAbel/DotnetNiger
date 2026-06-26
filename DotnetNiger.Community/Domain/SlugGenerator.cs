using System.Text.RegularExpressions;

namespace DotnetNiger.Community.Domain;

/// <summary>Génère des slugs SEO-friendly à partir de titres ou de textes.</summary>
public static partial class SlugGenerator
{
    /// <summary>Transforme un texte en slug URL-friendly (supprime accents, caractères spéciaux, espaces).</summary>
    /// <param name="text">Texte à convertir.</param>
    /// <returns>Slug nettoyé et en minuscules.</returns>
    public static string Generate(string text)
    {
        var slug = text.ToLowerInvariant()
            .Replace("'", "").Replace(".", "").Replace(",", "")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e")
            .Replace("à", "a").Replace("â", "a")
            .Replace("ù", "u").Replace("û", "u")
            .Replace("ô", "o").Replace("ö", "o")
            .Replace("î", "i").Replace("ï", "i")
            .Replace("ç", "c")
            .Replace("\"", "").Replace("'", "");

        slug = NonAlphanumericOrSpaceRegex().Replace(slug, "-");
        slug = ConsecutiveHyphensRegex().Replace(slug, "-");
        return slug.Trim('-');
    }

    [GeneratedRegex("[^a-z0-9\\-]")]
    private static partial Regex NonAlphanumericOrSpaceRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex ConsecutiveHyphensRegex();
}
