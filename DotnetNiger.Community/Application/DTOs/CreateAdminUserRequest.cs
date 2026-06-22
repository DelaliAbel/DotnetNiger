using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs;

public class CreateAdminUserRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
}
