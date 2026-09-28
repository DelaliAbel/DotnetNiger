using System.Globalization;
using System.Text;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Habillage commun à tous les courriels.
/// </summary>
/// <remarks>
/// Une seule fonction construit le bandeau, le pied et la version texte, afin que les dix-sept
/// modèles restent visuellement cohérents entre eux. Toutes les couleurs sont écrites en
/// style intégré : un client de messagerie comme Outlook ignore les feuilles de style, et
/// une image de fond disparaît chez la plupart des destinataires. C'est aussi pourquoi le
/// logo est suivi de son nom en toutes lettres : lorsque l'image est bloquée, le texte
/// prend le relais et le message reste identifiable.
/// </remarks>
internal static class EmailLayout
{
    /// <summary>Violet de la marque, celui du logo .NET.</summary>
    private const string Violet = "#512BD4";

    /// <summary>Violet éclairci, pour la bordure du bouton.</summary>
    private const string VioletClair = "#7B5FE0";

    /// <summary>Fond de la page autour du message.</summary>
    private const string FondPage = "#F6F5FA";

    /// <summary>Couleur du texte secondaire.</summary>
    private const string Gris = "#6B6B6B";

    /// <summary>Couleur de la bordure de la carte.</summary>
    private const string GrisClair = "#E4E1EE";

    /// <summary>
    /// Assemble le message final.
    /// </summary>
    /// <param name="objet">Objet du message.</param>
    /// <param name="contenu">Contenu habillé.</param>
    /// <param name="raison">Motif pour lequel le destinataire reçoit ce message.</param>
    /// <param name="branding">Identité du site.</param>
    /// <returns>Le message prêt à être expédié.</returns>
    public static EmailMessage Composer(
        string objet,
        EmailContent contenu,
        string raison,
        EmailBranding branding)
    {
        ArgumentNullException.ThrowIfNull(contenu);
        ArgumentNullException.ThrowIfNull(branding);
        ArgumentException.ThrowIfNullOrWhiteSpace(objet);
        ArgumentException.ThrowIfNullOrWhiteSpace(raison);

        var accent = contenu.Paragraphes.Count > 0 ? contenu.Paragraphes[0] : string.Empty;

        return new EmailMessage(
            AssainirObjet(objet),
            Html(objet, accent, contenu, raison, branding),
            Texte(objet, contenu, raison, branding));
    }

    /// <summary>
    /// Ramène l'objet sur une seule ligne.
    /// </summary>
    /// <param name="objet">Objet produit par le modèle.</param>
    /// <returns>Objet sans saut de ligne ni tabulation.</returns>
    /// <remarks>
    /// L'objet est le seul texte du message à finir dans un en-tête de courriel. Une valeur
    /// contenant un retour à la ligne pourrait y insérer un en-tête supplémentaire : le
    /// message resterait affichable, mais deviendrait falsifiable. Les sauts sont donc
    /// remplacés par un espace avant composition.
    /// </remarks>
    private static string AssainirObjet(string objet)
    {
        var tampon = new char[objet.Length];
        var longueur = 0;

        foreach (var caractere in objet)
        {
            tampon[longueur++] = caractere is '\r' or '\n' or '\t' ? ' ' : caractere;
        }

        return new string(tampon, 0, longueur);
    }

    private static string Html(
        string objet,
        string accent,
        EmailContent contenu,
        string raison,
        EmailBranding branding)
    {
        var html = new StringBuilder();

        html.Append(CultureInfo.InvariantCulture, $$"""
            <!DOCTYPE html>
            <html lang="fr">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width,initial-scale=1" />
              <title>{{EmailEncoding.Html.Encode(objet)}}</title>
            </head>
            <body style="margin:0;padding:0;background:{{FondPage}};font-family:Inter,'Segoe UI',system-ui,-apple-system,sans-serif;color:#1F1F1F;line-height:1.6;">
              <div style="display:none;max-height:0;max-width:0;overflow:hidden;opacity:0;">{{EmailEncoding.Html.Encode(accent)}}</div>
              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background:{{FondPage}};">
                <tr>
                  <td align="center" style="padding:32px 12px;">
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="600" style="width:100%;max-width:600px;background:#FFFFFF;border:1px solid {{GrisClair}};border-radius:12px;">
                      <tr>
                        <td align="center" style="padding:28px 32px 20px;border-bottom:1px solid {{GrisClair}};">

            """);

        // Le logo est une image : la plupart des clients la bloquent. Le nom du site est donc
        // écrit juste au-dessous, et sert de repli lisible lorsque l'image ne s'affiche pas.
        html.Append(CultureInfo.InvariantCulture, $$"""
                            <img src="{{EmailEncoding.Html.Encode(branding.LogoUrl)}}" alt="{{EmailEncoding.Html.Encode(EmailBranding.SiteName)}}" width="64" height="64" style="display:block;margin:0 auto 12px;border:0;" />
                            <div style="font-size:22px;font-weight:700;color:{{Violet}};letter-spacing:-0.2px;">{{EmailEncoding.Html.Encode(EmailBranding.SiteName)}}</div>
                            <div style="font-size:13px;color:{{Gris}};margin-top:2px;">{{EmailEncoding.Html.Encode(EmailBranding.Tagline)}}</div>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:28px 32px 8px;">

            """);

        html.Append(CultureInfo.InvariantCulture, $"""
                            <h1 style="margin:0 0 16px;font-size:20px;line-height:1.35;font-weight:700;">{EmailEncoding.Html.Encode(contenu.Titre)}</h1>
            """);

        for (var i = 0; i < contenu.Paragraphes.Count; i++)
        {
            var paragraphe = contenu.Paragraphes[i];

            if (i == 0)
            {
                html.Append(CultureInfo.InvariantCulture, $"""
                            <p style="margin:0 0 16px;font-size:16px;">{EmailEncoding.Html.Encode(paragraphe)}</p>
                """);
            }
            else
            {
                html.Append(CultureInfo.InvariantCulture, $"""
                            <p style="margin:0 0 16px;font-size:15px;color:#3A3A3A;">{EmailEncoding.Html.Encode(paragraphe)}</p>
                """);
            }
        }

        if (contenu.Appel is not null)
        {
            var url = EmailEncoding.Html.Encode(contenu.Appel.Url);
            var libelle = EmailEncoding.Html.Encode(contenu.Appel.Libelle);

            html.Append(CultureInfo.InvariantCulture, $"""

                            <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:8px 0 20px;">
                              <tr>
                                <td style="border-radius:8px;background:{Violet};box-shadow:0 2px 0 {VioletClair};">
                                  <a href="{url}" style="display:inline-block;padding:13px 26px;font-size:15px;font-weight:600;color:#FFFFFF;text-decoration:none;">{libelle}</a>
                                </td>
                              </tr>
                            </table>
                """);
        }

        if (!string.IsNullOrWhiteSpace(contenu.Note))
        {
            html.Append(CultureInfo.InvariantCulture, $"""
                            <p style="margin:4px 0 20px;font-size:13px;color:{Gris};">{EmailEncoding.Html.Encode(contenu.Note)}</p>
                """);
        }

        html.Append(CultureInfo.InvariantCulture, $"""
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:20px 32px 28px;border-top:1px solid {GrisClair};">

            """);

        // Motif de réception : exigé par les règles anti-spam, qui exigent aussi, pour la
        // lettre d'information, un lien de désinscription visible.
        html.Append(CultureInfo.InvariantCulture, $"""
                            <p style="margin:0 0 12px;font-size:13px;color:{Gris};">{EmailEncoding.Html.Encode(raison)}</p>
            """);

        var liens = new StringBuilder();

        foreach (var (libelle, chemin) in branding.LiensDuPied)
        {
            if (liens.Length > 0)
            {
                liens.Append(" &nbsp;·&nbsp; ");
            }

            var href = EmailEncoding.Html.Encode($"{branding.SiteUrl.TrimEnd('/')}{chemin}");

            liens.Append(CultureInfo.InvariantCulture,
                $"""<a href="{href}" style="color:{Violet};text-decoration:none;">{EmailEncoding.Html.Encode(libelle)}</a>""");
        }

        html.Append(CultureInfo.InvariantCulture, $"""
                            <p style="margin:0 0 12px;font-size:13px;">{liens}</p>
            """);

        if (!string.IsNullOrWhiteSpace(branding.ContactAddress))
        {
            var contact = EmailEncoding.Html.Encode(branding.ContactAddress);
            var hrefContact = EmailEncoding.Html.Encode($"mailto:{branding.ContactAddress}");

            html.Append(CultureInfo.InvariantCulture, $"""
                                <p style="margin:0 0 12px;font-size:13px;color:{Gris};">Une question ? Écrivez à <a href="{hrefContact}" style="color:{Violet};">{contact}</a></p>
                """);
        }

        if (!string.IsNullOrWhiteSpace(contenu.UrlDesinscription))
        {
            var desabonnement = EmailEncoding.Html.Encode(contenu.UrlDesinscription);

            html.Append(CultureInfo.InvariantCulture, $"""
                                <p style="margin:0 0 12px;font-size:13px;"><a href="{desabonnement}" style="color:{Violet};">Se désinscrire de la lettre d'information</a></p>
                """);
        }

        html.Append(CultureInfo.InvariantCulture, $"""
                            <p style="margin:0;padding-top:12px;border-top:1px solid {GrisClair};font-size:12px;color:{Gris};">
                              © {DateTime.UtcNow.Year} {EmailEncoding.Html.Encode(EmailBranding.SiteName)} — la communauté des développeurs .NET du Niger.
                            </p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            """);

        return html.ToString();
    }

    private static string Texte(
        string objet,
        EmailContent contenu,
        string raison,
        EmailBranding branding)
    {
        var texte = new StringBuilder();

        texte.AppendLine(EmailBranding.SiteName).AppendLine(EmailBranding.Tagline).AppendLine();
        texte.AppendLine(objet).AppendLine();
        texte.AppendLine(contenu.Titre).AppendLine();

        foreach (var paragraphe in contenu.Paragraphes)
        {
            texte.AppendLine(paragraphe).AppendLine();
        }

        if (contenu.Appel is not null)
        {
            texte.AppendLine(string.Create(
                CultureInfo.InvariantCulture,
                $"{contenu.Appel.Libelle} : {contenu.Appel.Url}"));
            texte.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(contenu.Note))
        {
            texte.AppendLine(contenu.Note).AppendLine();
        }

        texte.AppendLine("---").AppendLine();
        texte.AppendLine(raison).AppendLine();

        var liens = new List<string>(branding.LiensDuPied.Count);

        foreach (var (libelle, chemin) in branding.LiensDuPied)
        {
            liens.Add($"{libelle} : {branding.SiteUrl.TrimEnd('/')}{chemin}");
        }

        texte.AppendLine(string.Join(Environment.NewLine, liens));

        if (!string.IsNullOrWhiteSpace(branding.ContactAddress))
        {
            texte.AppendLine(string.Create(
                CultureInfo.InvariantCulture,
                $"Une question ? Écrivez à {branding.ContactAddress}"));
        }

        if (!string.IsNullOrWhiteSpace(contenu.UrlDesinscription))
        {
            texte.AppendLine(string.Create(
                CultureInfo.InvariantCulture,
                $"Se désinscrire : {contenu.UrlDesinscription}"));
        }

        texte.Append(CultureInfo.InvariantCulture, $"© {DateTime.UtcNow.Year} {EmailBranding.SiteName}");

        return texte.ToString();
    }
}
