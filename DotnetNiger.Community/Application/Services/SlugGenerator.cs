using System.Text;
using System.Text.RegularExpressions;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Application.Services;

public class SlugGenerator : ISlugGenerator
{
    private static readonly Regex InvalidCharsRegex = new("[^a-z0-9\\s-]", RegexOptions.Compiled);
    private static readonly Regex MultiSpaceRegex = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex MultiHyphenRegex = new("-+", RegexOptions.Compiled);

    public string Generate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Trim().ToLowerInvariant();
        normalized = RemoveDiacritics(normalized);
        normalized = InvalidCharsRegex.Replace(normalized, string.Empty);
        normalized = MultiSpaceRegex.Replace(normalized, "-");
        normalized = MultiHyphenRegex.Replace(normalized, "-");

        return normalized.Trim('-');
    }

    private static string RemoveDiacritics(string value)
    {
        var normalizedString = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
