using System.Text.RegularExpressions;

namespace DotnetNiger.UI.Helpers;

public static class HtmlSanitizer
{
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var sanitized = html;

        sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @" on\w+\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @" on\w+\s*=\s*'[^']*'", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"javascript\s*:", "", RegexOptions.IgnoreCase);

        return sanitized;
    }
}
