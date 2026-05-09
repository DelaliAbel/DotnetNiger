using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Dtos.Requests;

public class RegisterEventRequest
{
    [Required]
    public Guid EventId { get; set; }
}
