namespace DotnetNiger.Gateway.Application.Services.Interfaces;

/// <summary>
/// Interface pour le service de cache
/// </summary>
public interface ICachingService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}
