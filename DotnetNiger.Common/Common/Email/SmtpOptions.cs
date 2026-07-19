namespace DotnetNiger.Common.Email;

/// <summary>Options de configuration SMTP pour l'envoi d'emails.</summary>
public class SmtpOptions
{
    /// <summary>Hôte du serveur SMTP.</summary>
    public string Host { get; set; } = "";

    /// <summary>Port du serveur SMTP (défaut 587).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Nom d'utilisateur SMTP.</summary>
    public string Username { get; set; } = "";

    /// <summary>Mot de passe SMTP.</summary>
    public string Password { get; set; } = "";

    /// <summary>Email expéditeur.</summary>
    public string FromEmail { get; set; } = "noreply@dotnetniger.com";

    /// <summary>Nom affiché de l'expéditeur.</summary>
    public string FromName { get; set; } = "DotnetNiger";

    /// <summary>Nom de l'application.</summary>
    public string AppName { get; set; } = "DotnetNiger";

    /// <summary>Sous-titre de l'application (optionnel).</summary>
    public string AppSubtitle { get; set; } = "";

    /// <summary>URL de base de l'API Identity.</summary>
    public string AppBaseUrl { get; set; } = "";

    /// <summary>URL de base du frontend.</summary>
    public string FrontendBaseUrl { get; set; } = "";

    /// <summary>Email de support (optionnel).</summary>
    public string SupportEmail { get; set; } = "";
}
