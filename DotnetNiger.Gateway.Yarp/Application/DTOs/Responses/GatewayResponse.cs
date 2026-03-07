namespace DotnetNiger.Gateway.Application.DTOs.Responses;

/// <summary>
/// DTO pour les réponses du Gateway
/// </summary>
public class GatewayResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
}
