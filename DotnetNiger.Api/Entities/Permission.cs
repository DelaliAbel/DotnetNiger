namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente une permission système assignable aux rôles.
/// </summary>
public class Permission
{
    /// <summary>Identifiant unique de la permission.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom de la permission.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Catégorie de la permission.</summary>
    public string Category { get; set; } = string.Empty;
}
