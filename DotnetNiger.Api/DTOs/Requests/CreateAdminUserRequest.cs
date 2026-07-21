using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

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

    public bool IsCollaborator { get; set; }
    public bool IsAdmin { get; set; }
    public bool HasApprovedCertificate { get; set; }
}
