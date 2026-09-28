using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Encodeur partagé par tous les modèles de courriel.
/// </summary>
/// <remarks>
/// L'encodeur par défaut transforme « communauté » en « communaut&#xE9; ». Le message
/// s'affiche correctement, mais sa source devient illisible, il pèse plus lourd, et relire
/// un modèle hors d'un client de messagerie devient pénible. Les caractères latins
/// accentués sont donc laissés tels quels, seuls les caractères qui portent un sens en HTML
/// — <c>&lt;</c>, <c>&gt;</c>, <c>&amp;</c>, les guillemets — restent échappés.
/// </remarks>
internal static class EmailEncoding
{
    /// <summary>
    /// Encodeur HTML tolérant aux accents.
    /// </summary>
    public static HtmlEncoder Html { get; } = Creer();

    private static HtmlEncoder Creer()
    {
        var reglages = new TextEncoderSettings();

        foreach (var plage in new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement })
        {
            reglages.AllowRange(plage);
        }

        return HtmlEncoder.Create(reglages);
    }
}
