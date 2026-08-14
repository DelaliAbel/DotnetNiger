using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Options;

/// <summary>
/// Configuration du rate limiting injectée depuis appsettings.json (section "RateLimiting").
/// </summary>
public class RateLimitingOptions
{
    /// <summary>Nom de la section dans appsettings.json.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>Nombre max de requêtes autorisées par fenêtre (policy par défaut).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "RateLimiting:PermitLimit doit être supérieur à 0.")]
    public int PermitLimit { get; set; } = 5;

    /// <summary>Durée de la fenêtre en secondes (policy par défaut).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "RateLimiting:WindowSeconds doit être supérieur à 0.")]
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Nombre max de requêtes autorisées par fenêtre pour l'authentification.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "RateLimiting:AuthPermitLimit doit être supérieur à 0.")]
    public int AuthPermitLimit { get; set; } = 10;

    /// <summary>Durée de la fenêtre en secondes pour l'authentification.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "RateLimiting:AuthWindowSeconds doit être supérieur à 0.")]
    public int AuthWindowSeconds { get; set; } = 60;

    /// <summary>Nombre max de requêtes autorisées par fenêtre pour l'ensemble de l'API (fallback global).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "RateLimiting:GlobalPermitLimit doit être supérieur à 0.")]
    public int GlobalPermitLimit { get; set; } = 100;
}
