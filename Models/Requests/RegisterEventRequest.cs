using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class RegisterEventRequest
{
    [Required]
    public Guid EventId { get; set; }
}
