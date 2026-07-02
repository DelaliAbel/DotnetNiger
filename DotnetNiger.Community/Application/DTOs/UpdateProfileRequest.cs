namespace DotnetNiger.Community.Application.DTOs;

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool? IsTeamMember { get; set; }
    public string? Position { get; set; }
    public List<string>? Skills { get; set; }
}
