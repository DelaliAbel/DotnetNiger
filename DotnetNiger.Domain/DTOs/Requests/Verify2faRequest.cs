namespace DotnetNiger.Domain.DTOs.Requests;

public class Verify2faRequest
{
    public string Code { get; set; } = string.Empty;
    public string ChallengeToken { get; set; } = string.Empty;
    public bool RememberMachine { get; set; }
}
