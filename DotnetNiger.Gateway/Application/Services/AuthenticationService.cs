namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Valide les JWT tokens
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        // TODO: Implémenter la validation JWT réelle
        _logger.LogInformation("Validation du token JWT");
        return Task.FromResult(true);
    }

    public Task<string?> GetUserIdFromTokenAsync(string token)
    {
        // TODO: Extraire l'userId du token JWT
        return Task.FromResult<string?>(null);
    }
}
