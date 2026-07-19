namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête de désactivation de l'authentification à deux facteurs.
/// </summary>
public class Disable2faRequest
{
    public string Code { get; set; } = string.Empty;
}
