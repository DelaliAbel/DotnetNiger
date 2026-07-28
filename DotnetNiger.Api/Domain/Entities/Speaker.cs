namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente un intervenant associé à un événement.
/// </summary>
public class Speaker
{
    /// <summary>Identifiant unique de l'intervenant.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'événement.</summary>
    public Guid EventId { get; set; }
    /// <summary>Identifiant de l'utilisateur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom de l'intervenant.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Biographie de l'intervenant.</summary>
    public string Bio { get; set; } = string.Empty;
    /// <summary>Rôle lors de l'événement.</summary>
    public string Role { get; set; } = string.Empty;
    /// <summary>URL de l'avatar.</summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>Navigation vers l'événement.</summary>
    public Event Event { get; set; } = null!;
}
