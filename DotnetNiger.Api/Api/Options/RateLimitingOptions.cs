namespace DotnetNiger.Api.Options;

/// <summary>
/// Configuration du rate limiting injectée depuis appsettings.json (section "RateLimiting").
/// </summary>
public class RateLimitingOptions
{
    /// <summary>Nom de la section dans appsettings.json.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>Nombre max de requêtes autorisées par fenêtre (policy par défaut).</summary>
    public int PermitLimit { get; set; } = 5;

    /// <summary>Durée de la fenêtre en secondes (policy par défaut).</summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Nombre max de requêtes autorisées par fenêtre pour l'authentification.</summary>
    public int AuthPermitLimit { get; set; } = 10;

    /// <summary>Durée de la fenêtre en secondes pour l'authentification.</summary>
    public int AuthWindowSeconds { get; set; } = 60;
}
