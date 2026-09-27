namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Motif de signalement, propose a l'utilisateur puis affine par la moderation.
/// </summary>
public enum ReportReason
{
    /// <summary>Publicite, contenu hors sujet ou doublon.</summary>
    Spam = 0,

    /// <summary>Harcèlement, insultes ou propos discriminatoires.</summary>
    Harassment = 1,

    /// <summary>Contenu violent, sexuel ou choquant.</summary>
    InappropriateContent = 2,

    /// <summary>Information factuellement incorrecte.</summary>
    IncorrectInformation = 3,

    /// <summary>Signalement express, sans motifstandard.</summary>
    Other = 4,
}
