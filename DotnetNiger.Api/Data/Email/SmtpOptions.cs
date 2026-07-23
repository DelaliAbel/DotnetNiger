namespace DotnetNiger.Api.Data.Email;

/// <summary>
/// Options de configuration du serveur SMTP pour l'envoi d'emails.
/// </summary>
public class SmtpOptions
{
    /// <summary>Adresse du serveur SMTP.</summary>
    public string Host { get; set; } = "";
    /// <summary>Port du serveur SMTP.</summary>
    public int Port { get; set; } = 587;
    /// <summary>Nom d'utilisateur SMTP.</summary>
    public string Username { get; set; } = "";
    /// <summary>Mot de passe SMTP.</summary>
    public string Password { get; set; } = "";
    /// <summary>Adresse email de l'expéditeur.</summary>
    public string FromEmail { get; set; } = "noreply@dotnetniger.com";
    /// <summary>Nom de l'expéditeur.</summary>
    public string FromName { get; set; } = "DotnetNiger Community";
    /// <summary>Nom de l'application pour les emails.</summary>
    public string AppName { get; set; } = "DotnetNiger Community";
    /// <summary>Sous-titre de l'application dans les emails.</summary>
    public string AppSubtitle { get; set; } = "";
    /// <summary>URL de base de l'API.</summary>
    public string AppBaseUrl { get; set; } = "";
    /// <summary>URL de base du frontend.</summary>
    public string FrontendBaseUrl { get; set; } = "";
    /// <summary>Email du support.</summary>
    public string SupportEmail { get; set; } = "";
}
