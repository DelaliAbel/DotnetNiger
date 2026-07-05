namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête de vérification du code à deux facteurs.
/// </summary>
public class Verify2faRequest
{
    public string Code { get; set; } = string.Empty;
    public string ChallengeToken { get; set; } = string.Empty;
    public bool RememberMachine { get; set; }
}
