namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Objet et corps d'un message à expédier.
/// </summary>
/// <param name="Subject">Objet du message.</param>
/// <param name="HtmlBody">Corps au format HTML.</param>
/// <param name="TextBody">
/// Corps au format texte brut. Il n'est pas facultatif : beaucoup de clients
/// professionnels et de filtres antispam le privilégient, et il reste lisible quand une
/// politique de sécurité bloque la version HTML.
/// </param>
public sealed record EmailMessage(string Subject, string HtmlBody, string TextBody);
