using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'inscription à un événement.</summary>
public class RegisterEventRequest
{
    /// <summary>Identifiant de l'événement.</summary>
    [Required]
    public Guid EventId { get; set; }

    /// <summary>URL de l'avatar du participant.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
}
