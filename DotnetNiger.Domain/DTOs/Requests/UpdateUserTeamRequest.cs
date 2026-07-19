namespace DotnetNiger.Domain.DTOs.Requests;

public class UpdateUserTeamRequest
{
    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
}
