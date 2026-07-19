using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Client.Models.Requests;

public class SubscribeRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
