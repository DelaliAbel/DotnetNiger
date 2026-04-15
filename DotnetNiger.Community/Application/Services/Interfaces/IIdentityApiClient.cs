namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IIdentityApiClient
{
    Task<bool> IsReachableAsync(CancellationToken cancellationToken = default);
    Task<Guid?> RegisterAsync(IdentityRegisterRequest request, CancellationToken cancellationToken = default);
    string BaseUrl { get; }
}

public class IdentityRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
