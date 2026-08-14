using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotnetNiger.Api.Infrastructure.Auth;

/// <summary>
/// Service natif Microsoft pour la gestion des tokens JWT.
/// Génère les access tokens et gère le cycle de vie des refresh tokens.
/// Remplace totalement OpenIddict pour la création de tokens.
/// </summary>
public class TokenService
{
    private readonly JwtSettings _jwt;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;
    private readonly DotnetNigerDbContext _db;

    public TokenService(
        IOptions<JwtSettings> jwt,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService,
        DotnetNigerDbContext db)
    {
        _jwt = jwt.Value;
        _userManager = userManager;
        _permissionService = permissionService;
        _db = db;
    }

    /// <summary>
    /// Génère un access token JWT signé + un refresh token pour un utilisateur.
    /// Les rôles et permissions sont injectés dans les claims du JWT.
    /// </summary>
    public async Task<(string accessToken, string refreshToken, int expiresIn)> GenerateTokenPairAsync(
        ApplicationUser user, bool rememberMe = false)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        var accessToken = GenerateAccessToken(user, roles, permissions);
        var refreshToken = await CreateRefreshTokenAsync(user, rememberMe);
        var expiresIn = rememberMe
            ? (int)TimeSpan.FromDays(7).TotalSeconds
            : _jwt.AccessTokenExpirationMinutes * 60;

        return (accessToken, refreshToken, expiresIn);
    }

    /// <summary>
    /// Génère un access token JWT signé avec les claims de l'utilisateur.
    /// Contient : sub, email, name, roles (natifs), permissions (custom).
    /// </summary>
    private string GenerateAccessToken(
        ApplicationUser user, IList<string> roles, IList<string> permissions)
    {
        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Rôles natifs Microsoft (ClaimTypes.Role pour [Authorize(Roles=)])
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Permissions custom (claim "permission" pour PermissionAuthorizationHandler)
        foreach (var perm in permissions)
            claims.Add(new Claim("permission", perm));

        var expiration = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Crée un refresh token, le stocke en base et retourne la valeur brute.
    /// Le token est stocké sous forme hashée (SHA256) pour la sécurité.
    /// </summary>
    private async Task<string> CreateRefreshTokenAsync(ApplicationUser user, bool longLived)
    {
        var rawToken = GenerateRandomToken();
        var hash = HashToken(rawToken);
        var expiration = longLived
            ? DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays * 4)
            : DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = expiration,
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return rawToken;
    }

    /// <summary>
    /// Valide un refresh token et retourne l'utilisateur associé.
    /// Vérifie : existence, expiration, révocation.
    /// </summary>
    public async Task<ApplicationUser?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var entity = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (entity is null)
            return null;

        if (entity.RevokedAt.HasValue || entity.IsReplaced)
            return null;

        if (entity.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await _userManager.FindByIdAsync(entity.UserId.ToString());
        if (user is null || !user.IsActive)
            return null;

        return user;
    }

    /// <summary>
    /// Effectue la rotation du refresh token : révance l'ancien et crée un nouveau.
    /// Retourne le nouveau couple (accessToken, refreshToken, expiresIn).
    /// </summary>
    public async Task<(string accessToken, string refreshToken, int expiresIn)?>
        RotateRefreshTokenAsync(string oldRefreshToken, bool rememberMe = false)
    {
        var hash = HashToken(oldRefreshToken);
        var entity = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (entity is null || entity.RevokedAt.HasValue || entity.IsReplaced)
            return null;

        if (entity.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await _userManager.FindByIdAsync(entity.UserId.ToString());
        if (user is null || !user.IsActive)
            return null;

        // Révoque l'ancien token
        entity.IsReplaced = true;
        entity.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Génère un nouveau couple de tokens
        return await GenerateTokenPairAsync(user, rememberMe);
    }

    /// <summary>
    /// Révoque un refresh token spécifique (déconnexion de cet appareil).
    /// </summary>
    public async Task RevokeTokenAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var entity = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (entity is not null)
        {
            entity.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>Génère une chaîne aléatoire cryptographiquement sûre (64 chars hex).</summary>
    private static string GenerateRandomToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>Hash SHA256 d'un token pour le stockage sécurisé en base.</summary>
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
