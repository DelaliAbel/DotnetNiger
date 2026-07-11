using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetNiger.Common.Extensions;

public static class SlugGenerator
{
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormKD);
        var sb = new StringBuilder(capacity: normalized.Length);

        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormKC);
        slug = slug.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        slug = Regex.Replace(slug, @"-+", "-");

        if (string.IsNullOrWhiteSpace(slug))
            return $"slug-{Guid.NewGuid():N}";

        return slug;
    }
}
