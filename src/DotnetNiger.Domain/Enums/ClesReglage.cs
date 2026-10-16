namespace DotnetNiger.Domain.Enums;

/// <summary>Clés de réglage connues du site.</summary>
/// <remarks>
/// Elles sont nommées ici plutôt que laissées libres dans les appelants : une clé écrite à la
/// main finit toujours par diverger d'un caractère, et le réglage devient alors
/// silencieusement inopérant. Toute clé absente de cette liste est ignorée.
/// </remarks>
public static class ClesReglage
{
    /// <summary>Logo du site, en-tête et courriels.</summary>
    public const string Logo = "site.logo";

    /// <summary>Adresse de contact affichée sur le site et utilisée dans les courriels.</summary>
    public const string Courriel = "site.courriel";

    /// <summary>Numéro de téléphone de la communauté, affiché sur le site.</summary>
    public const string Telephone = "site.telephone";

    /// <summary>Adresse du numéro de la communauté sur WhatsApp.</summary>
    public const string Whatsapp = "canal.whatsapp";

    /// <summary>Dépôt GitHub de la communauté.</summary>
    public const string Github = "canal.github";

    /// <summary>Page LinkedIn de la communauté.</summary>
    public const string Linkedin = "canal.linkedin";

    /// <summary>Compte X de la communauté.</summary>
    public const string X = "canal.x";

    /// <summary>Chaîne YouTube de la communauté.</summary>
    public const string Youtube = "canal.youtube";

    /// <summary>Page Facebook de la communauté.</summary>
    public const string Facebook = "canal.facebook";

    /// <summary>Toutes les clés, dans l'ordre où l'écran Paramètres les présente.</summary>
    public static readonly string[] Toutes =
    [
        Logo, Courriel, Telephone, Whatsapp, Github, Linkedin, X, Youtube, Facebook,
    ];
}