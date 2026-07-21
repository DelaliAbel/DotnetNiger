using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class RegisterEventRequest
{
    [Required]
    public Guid EventId { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;
}
