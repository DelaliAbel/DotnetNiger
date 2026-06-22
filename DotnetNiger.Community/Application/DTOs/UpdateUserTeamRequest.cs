namespace DotnetNiger.Community.Application.DTOs;

public class UpdateUserTeamRequest
{
    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
}
