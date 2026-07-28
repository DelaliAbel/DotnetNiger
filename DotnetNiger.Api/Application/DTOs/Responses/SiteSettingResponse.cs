namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'un paramètre du site.</summary>
public class SiteSettingResponse
{
    /// <summary>Clé du paramètre.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Valeur du paramètre.</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Type de la valeur (string, bool, int, etc.).</summary>
    public string Type { get; set; } = "string";
    /// <summary>Description du paramètre.</summary>
    public string? Description { get; set; }
}
