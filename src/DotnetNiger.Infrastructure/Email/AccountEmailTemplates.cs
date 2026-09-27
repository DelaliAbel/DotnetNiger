using System.Text.Encodings.Web;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Fabrique des messages du parcours de compte.
/// </summary>
/// <remarks>
/// Les corps sont produits en C# plutôt que lus dans des fichiers .cshtml : un message
/// expédié ne passe pas par le pipeline Razor, et ajouter un moteur de rendu pour cinq
/// textes n'apporterait rien. Les adresses de lien sont fournies par l'appelant, qui
/// connaît la configuration du site.
/// </remarks>
public static class AccountEmailTemplates
{
    /// <summary>
    /// Message de confirmation d'inscription.
    /// </summary>
    /// <param name="displayName">Prénom du membre.</param>
    /// <param name="confirmationUrl">Lien de confirmation.</param>
    /// <returns>Objet et corps du message.</returns>
    public static EmailMessage BuildConfirmation(string displayName, string confirmationUrl) =>
        new(
            "Confirmez votre adresse sur dotnetNiger",
            Compose(
                $"Bonjour {displayName},",
                "Merci de votre inscription. Confirmez votre adresse pour activer votre compte et commencer à commenter, publier des ressources et vous inscrire aux événements.",
                "Confirmer mon adresse",
                confirmationUrl,
                "Si vous n'êtes pas à l'origine de cette inscription, ignorez ce message."));

    /// <summary>
    /// Message de bienvenue, envoyé après la confirmation de l'adresse.
    /// </summary>
    /// <param name="displayName">Prénom du membre.</param>
    /// <param name="communityUrl">Adresse de la page communauté.</param>
    /// <returns>Objet et corps du message.</returns>
    public static EmailMessage BuildWelcome(string displayName, string communityUrl) =>
        new(
            "Bienvenue sur dotnetNiger",
            Compose(
                $"Bonjour {displayName},",
                "Votre adresse est confirmée et votre compte est actif. La communauté dotNetNiger réunit les développeurs .NET du Niger autour d'événements, de ressources et de projets open source.",
                "Découvrir la communauté",
                communityUrl,
                "Votre inscription à la lettre d'information reste facultative."));

    /// <summary>
    /// Message de réinitialisation de mot de passe.
    /// </summary>
    /// <param name="displayName">Prénom du membre.</param>
    /// <param name="resetUrl">Lien de réinitialisation.</param>
    /// <returns>Objet et corps du message.</returns>
    public static EmailMessage BuildPasswordReset(string displayName, string resetUrl) =>
        new(
            "Réinitialisez votre mot de passe dotnetNiger",
            Compose(
                $"Bonjour {displayName},",
                "Une réinitialisation de mot de passe a été demandée pour votre compte.",
                "Choisir un nouveau mot de passe",
                resetUrl,
                "Ce lien expire. S'il n'a pas été demandé, ignorez ce message : votre mot de passe reste inchangé."));

    private static string Compose(
        string salutation,
        string corps,
        string libelleBouton,
        string url,
        string pied) => $$"""
        <!DOCTYPE html>
        <html lang="fr">
        <head><meta charset="utf-8" /></head>
        <body style="margin:0;padding:24px;background:#ffffff;font-family:Inter,Segoe UI,system-ui,sans-serif;color:#1f1f1f;line-height:1.6;">
          <div style="max-width:560px;margin:0 auto;">
            <p style="color:#512BD4;font-weight:600;margin:0 0 24px;">dotnetNiger</p>
            <p style="margin:0 0 16px;">{{HtmlEncoder.Default.Encode(salutation)}}</p>
            <p style="margin:0 0 24px;">{{HtmlEncoder.Default.Encode(corps)}}</p>
            <p style="margin:0 0 24px;">
              <a href="{{HtmlEncoder.Default.Encode(url)}}"
                 style="display:inline-block;background:#512BD4;color:#ffffff;padding:12px 24px;border-radius:8px;text-decoration:none;">
                {{HtmlEncoder.Default.Encode(libelleBouton)}}
              </a>
            </p>
            <p style="color:#6b6b6b;font-size:14px;margin:0;">{{HtmlEncoder.Default.Encode(pied)}}</p>
          </div>
        </body>
        </html>
        """;
}
