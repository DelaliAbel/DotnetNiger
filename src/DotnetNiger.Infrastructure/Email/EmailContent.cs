namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Contenu d'un modèle, avant habillage par la mise en page commune.
/// </summary>
/// <param name="Titre">Titre du message, affiché au-dessus des paragraphes.</param>
/// <param name="Paragraphes">Paragraphe d'accroche, puis corps du message.</param>
/// <param name="Appel">Bouton d'action principale, facultatif.</param>
/// <param name="Note">Précision discrète placée avant le pied de message, facultative.</param>
/// <param name="UrlDesinscription">
/// Adresse de désinscription propre à ce destinataire, ou <see langword="null"/> si le
/// message n'est pas une diffusion. La lettre d'information doit la proposer : les règles
/// anti-spam l'exigent, et c'est aussi le seul moyen, pour un abonné, de se soustraire à un
/// envoi qu'il ne souhaite plus recevoir.
/// </param>
public sealed record EmailContent(
    string Titre,
    IReadOnlyList<string> Paragraphes,
    EmailAction? Appel = null,
    string? Note = null,
    string? UrlDesinscription = null);

/// <summary>
/// Bouton d'action principale d'un message.
/// </summary>
/// <param name="Libelle">Texte du bouton.</param>
/// <param name="Url">Adresse absolue appelée par le bouton.</param>
public sealed record EmailAction(string Libelle, string Url);
