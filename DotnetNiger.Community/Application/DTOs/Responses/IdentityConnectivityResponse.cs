namespace DotnetNiger.Community.Application.DTOs.Responses;

public class IdentityConnectivityResponse
{
    public bool Reachable { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public DateTime CheckedAtUtc { get; set; }
}
