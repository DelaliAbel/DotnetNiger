using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public class RegisterEventRequest
{
    [Required]
    public Guid EventId { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;
}
