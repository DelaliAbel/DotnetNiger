using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetNiger.Gateway.Application.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Valide les JWT tokens générés par le service Identity.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _validationParameters;

    public AuthenticationService(ILogger<AuthenticationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || key.StartsWith("__"))
        {
            _logger.LogWarning("Clé JWT non configurée - la validation sera désactivée");
            _validationParameters = null!;
        }
        else
        {
            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        if (_validationParameters == null)
        {
            _logger.LogWarning("Validation JWT désactivée - clé non configurée");
            return Task.FromResult(false);
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);

            _logger.LogDebug("Token JWT validé avec succès");
            return Task.FromResult(true);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token JWT expiré");
            return Task.FromResult(false);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogDebug("Token JWT invalide: {Message}", ex.Message);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du token JWT");
            return Task.FromResult(false);
        }
    }

    public Task<string?> GetUserIdFromTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult<string?>(null);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userId = jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                ?.Value;

            return Task.FromResult(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction de l'userId du token");
            return Task.FromResult<string?>(null);
        }
    }

    /// <summary>
    /// Extrait tous les claims du token pour enrichir le contexte
    /// </summary>
    public ClaimsPrincipal? GetClaimsPrincipal(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || _validationParameters == null)
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
