namespace DotnetNiger.Community.Infrastructure.External;

public interface IIdentityApiClient
{
    Task<UserDto?> GetUserAsync(string userId, CancellationToken ct = default);
    Task<RoleDto[]> GetUserRolesAsync(string userId, CancellationToken ct = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
    Task<bool> IsReachableAsync(CancellationToken ct = default);
    Task<HealthCheckDto?> GetHealthAsync(CancellationToken ct = default);
}
