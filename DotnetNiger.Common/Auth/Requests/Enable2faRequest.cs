namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête d'activation de l'authentification à deux facteurs.
/// </summary>
public class Enable2faRequest
{
    public string Code { get; set; } = string.Empty;
}
