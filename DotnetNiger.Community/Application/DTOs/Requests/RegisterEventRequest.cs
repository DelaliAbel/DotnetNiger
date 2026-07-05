using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête d'inscription à un événement.</summary>
public class RegisterEventRequest
{
    [Required]
    public Guid EventId { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;
}
