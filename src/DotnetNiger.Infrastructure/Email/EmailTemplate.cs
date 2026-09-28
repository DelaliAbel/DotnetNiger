namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Un modèle de courriel, décrit une fois et rendu à la demande.
/// </summary>
/// <param name="Cle">Clé du modèle, telle qu'elle figure dans la file d'attente.</param>
/// <param name="Titre">Intitulé lisible, destiné à la liste d'aperçu de l'administration.</param>
/// <param name="Usage">Phrase expliquant quand ce message est envoyé.</param>
/// <param name="Raison">Motif de réception, affiché en pied de message.</param>
/// <param name="Variables">Noms des variables attendues.</param>
/// <param name="Objet">Fabrique de l'objet du message.</param>
/// <param name="Contenu">Fabrique du contenu habillé.</param>
/// <param name="Exemple">Jeu de valeurs servant à l'aperçu et aux tests.</param>
public sealed record EmailTemplate(
    string Cle,
    string Titre,
    string Usage,
    string Raison,
    IReadOnlyList<string> Variables,
    Func<EmailValues, string> Objet,
    Func<EmailValues, EmailContent> Contenu,
    EmailValues Exemple)
{
    /// <summary>
    /// Rend le message complet pour un destinataire donné.
    /// </summary>
    /// <param name="valeurs">Valeurs du message.</param>
    /// <param name="branding">Identité du site.</param>
    /// <returns>Le message prêt à être expédié.</returns>
    public EmailMessage Rendre(EmailValues valeurs, EmailBranding branding)
    {
        ArgumentNullException.ThrowIfNull(valeurs);
        ArgumentNullException.ThrowIfNull(branding);

        return EmailLayout.Composer(
            Objet(valeurs),
            Contenu(valeurs),
            Raison,
            branding);
    }

    /// <summary>
    /// Rend le message avec les valeurs d'exemple, pour l'aperçu.
    /// </summary>
    /// <param name="branding">Identité du site.</param>
    /// <returns>Le message d'exemple.</returns>
    public EmailMessage RendreExemple(EmailBranding branding) => Rendre(Exemple, branding);
}
