namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un paramètre de configuration du site.
/// </summary>
public class SiteSetting
{
    /// <summary>Identifiant du paramètre (clé unique).</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Clé du paramètre.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Valeur du paramètre.</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Type de la valeur (string, bool, int, etc.).</summary>
    public string Type { get; set; } = "string";
    /// <summary>Description du paramètre.</summary>
    public string? Description { get; set; }
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; }
}
