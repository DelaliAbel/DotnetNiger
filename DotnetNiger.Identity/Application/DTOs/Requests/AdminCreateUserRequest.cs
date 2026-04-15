namespace DotnetNiger.Identity.Application.DTOs.Requests;

public class AdminCreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
    public string RoleName { get; set; } = "Member";
}
