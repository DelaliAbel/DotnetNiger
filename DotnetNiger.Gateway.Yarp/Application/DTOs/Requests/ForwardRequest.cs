namespace DotnetNiger.Gateway.Application.DTOs.Requests;

/// <summary>
/// DTO pour les requêtes de forwarding
/// </summary>
public class ForwardRequest
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
}
