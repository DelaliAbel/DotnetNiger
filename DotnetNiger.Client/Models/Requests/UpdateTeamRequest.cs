namespace DotnetNiger.Client.Models.Requests;

public class UpdateTeamRequest
{
    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
}
