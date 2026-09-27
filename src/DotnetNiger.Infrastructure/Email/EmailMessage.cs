namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Objet et corps d'un message à expédier.
/// </summary>
/// <param name="Subject">Objet du message.</param>
/// <param name="HtmlBody">Corps au format HTML.</param>
public sealed record EmailMessage(string Subject, string HtmlBody);
