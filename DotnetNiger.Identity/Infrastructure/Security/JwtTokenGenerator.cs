// Composant securite Identity: JwtTokenGenerator
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotnetNiger.Identity.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    // Generation du token d'acces JWT.
    private readonly JwtOptions _options;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DotnetNigerIdentityDbContext _dbContext;

    public JwtTokenGenerator(IOptions<JwtOptions> options, UserManager<ApplicationUser> userManager, DotnetNigerIdentityDbContext dbContext)
    {
        _options = options.Value;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
        var isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("is_admin", isAdmin ? "true" : "false"),
            new Claim("is_super_admin", isSuperAdmin ? "true" : "false")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var roleIds = await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.Name != null && roles.Contains(role.Name))
            .Select(role => role.Id)
            .ToListAsync();

        var permissions = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
            .Join(
                _dbContext.Permissions.AsNoTracking(),
                rolePermission => rolePermission.PermissionId,
                permission => permission.Id,
                (_, permission) => permission.Name)
            .Distinct()
            .ToListAsync();

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
