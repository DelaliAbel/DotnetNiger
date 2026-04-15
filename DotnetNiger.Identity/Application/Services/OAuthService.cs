using System.Text.Json;
using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Mappers;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

public class OAuthService : IOAuthService
{
    private static readonly string[] SupportedProviders = ["google", "github", "microsoft"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly DotnetNigerIdentityDbContext _dbContext;
    private readonly IRefreshTokenPersistence _refreshTokenRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly RefreshTokenGenerator _refreshTokenGenerator;
    private readonly IAppSettingPersistence _appSettingRepository;
    private readonly IConfiguration _configuration;
    private readonly JwtOptions _jwtOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OAuthService(
        IHttpClientFactory httpClientFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        DotnetNigerIdentityDbContext dbContext,
        IRefreshTokenPersistence refreshTokenRepository,
        JwtTokenGenerator jwtTokenGenerator,
        RefreshTokenGenerator refreshTokenGenerator,
        IAppSettingPersistence appSettingRepository,
        IConfiguration configuration,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _appSettingRepository = appSettingRepository;
        _configuration = configuration;
        _jwtOptions = jwtOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<IReadOnlyList<string>> GetEnabledProvidersAsync()
    {
        var enabled = SupportedProviders
            .Where(IsProviderEnabled)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(enabled);
    }

    public async Task<AuthResponse> ExchangeAccessTokenAsync(OAuthExchangeRequest request, CancellationToken ct = default)
    {
        var provider = NormalizeProvider(request.Provider);
        if (provider is null)
        {
            throw new IdentityException("Unsupported OAuth provider.", StatusCodes.Status400BadRequest);
        }

        if (!IsProviderEnabled(provider))
        {
            throw new IdentityException($"OAuth provider '{provider}' is disabled.", StatusCodes.Status503ServiceUnavailable);
        }

        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            throw new IdentityException("Access token is required.", StatusCodes.Status400BadRequest);
        }

        var external = await GetExternalIdentityAsync(provider, request.AccessToken, ct);

        var user = await _userManager.FindByLoginAsync(provider, external.ProviderUserId);
        if (user is null && !string.IsNullOrWhiteSpace(external.Email))
        {
            user = await _userManager.FindByEmailAsync(external.Email);
        }

        if (user is null)
        {
            user = await CreateUserFromExternalIdentityAsync(provider, external);
        }
        else
        {
            var existingLogins = await _userManager.GetLoginsAsync(user);
            if (!existingLogins.Any(login => login.LoginProvider.Equals(provider, StringComparison.OrdinalIgnoreCase)
                && login.ProviderKey == external.ProviderUserId))
            {
                var linkResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, external.ProviderUserId, provider));
                if (!linkResult.Succeeded)
                {
                    var message = string.Join(" ", linkResult.Errors.Select(error => error.Description));
                    throw new IdentityException(message, StatusCodes.Status400BadRequest);
                }
            }
        }

        if (!user.IsActive || user.IsDeleted)
        {
            throw new IdentityException("User account is inactive.", StatusCodes.Status403Forbidden);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = await CreateTokenAsync(user);
        var userDto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);

        return new AuthResponse
        {
            User = userDto,
            Token = token
        };
    }

    private async Task<ApplicationUser> CreateUserFromExternalIdentityAsync(string provider, ExternalIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(identity.Email))
        {
            throw new IdentityException("External provider did not return an email.", StatusCodes.Status400BadRequest);
        }

        var baseUsername = BuildBaseUsername(identity.Email, identity.Name);
        var username = await EnsureUniqueUsernameAsync(baseUsername);

        var user = new ApplicationUser
        {
            UserName = username,
            Email = identity.Email,
            FullName = string.IsNullOrWhiteSpace(identity.Name) ? username : identity.Name,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(error => error.Description));
            throw new IdentityException(message, StatusCodes.Status400BadRequest);
        }

        var defaultRole = _appSettingRepository.GetValue("Auth:DefaultRole")
            ?? _configuration["Auth:DefaultRole"]
            ?? "Member";

        var allowedRoles = new[] { "Member", "Admin", "SuperAdmin" };
        if (!allowedRoles.Contains(defaultRole, StringComparer.OrdinalIgnoreCase))
        {
            defaultRole = "Member";
        }

        if (await _roleManager.RoleExistsAsync(defaultRole))
        {
            await _userManager.AddToRoleAsync(user, defaultRole);
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, identity.ProviderUserId, provider));
        if (!addLoginResult.Succeeded)
        {
            var message = string.Join(" ", addLoginResult.Errors.Select(error => error.Description));
            throw new IdentityException(message, StatusCodes.Status400BadRequest);
        }

        return user;
    }

    private async Task<TokenResponse> CreateTokenAsync(ApplicationUser user)
    {
        var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
        var refreshTokenValue = _refreshTokenGenerator.GenerateToken();
        var hashedToken = RefreshTokenGenerator.HashToken(refreshTokenValue);

        var httpContext = _httpContextAccessor.HttpContext;
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = _jwtOptions.AccessTokenMinutes * 60,
            TokenType = "Bearer"
        };
    }

    private async Task<ExternalIdentity> GetExternalIdentityAsync(string provider, string accessToken, CancellationToken ct)
    {
        return provider switch
        {
            "google" => await GetGoogleIdentityAsync(accessToken, ct),
            "github" => await GetGitHubIdentityAsync(accessToken, ct),
            "microsoft" => await GetMicrosoftIdentityAsync(accessToken, ct),
            _ => throw new IdentityException("Unsupported OAuth provider.", StatusCodes.Status400BadRequest)
        };
    }

    private async Task<ExternalIdentity> GetGoogleIdentityAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var client = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new IdentityException("Invalid Google access token.", StatusCodes.Status401Unauthorized);
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;

        return new ExternalIdentity
        {
            ProviderUserId = root.GetProperty("sub").GetString() ?? string.Empty,
            Email = root.TryGetProperty("email", out var email) ? email.GetString() : null,
            Name = root.TryGetProperty("name", out var name) ? name.GetString() : null
        };
    }

    private async Task<ExternalIdentity> GetGitHubIdentityAsync(string accessToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DotnetNiger-Identity");

        using var userResponse = await client.GetAsync("https://api.github.com/user", ct);
        if (!userResponse.IsSuccessStatusCode)
        {
            throw new IdentityException("Invalid GitHub access token.", StatusCodes.Status401Unauthorized);
        }

        using var userDoc = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync(ct));
        var userRoot = userDoc.RootElement;

        string? email = userRoot.TryGetProperty("email", out var emailNode) ? emailNode.GetString() : null;
        if (string.IsNullOrWhiteSpace(email))
        {
            using var emailsResponse = await client.GetAsync("https://api.github.com/user/emails", ct);
            if (emailsResponse.IsSuccessStatusCode)
            {
                using var emailsDoc = JsonDocument.Parse(await emailsResponse.Content.ReadAsStringAsync(ct));
                var primary = emailsDoc.RootElement.EnumerateArray()
                    .FirstOrDefault(item => item.TryGetProperty("primary", out var p) && p.GetBoolean());

                if (primary.ValueKind != JsonValueKind.Undefined && primary.TryGetProperty("email", out var primaryEmail))
                {
                    email = primaryEmail.GetString();
                }
            }
        }

        return new ExternalIdentity
        {
            ProviderUserId = userRoot.GetProperty("id").ToString(),
            Email = email,
            Name = userRoot.TryGetProperty("name", out var name) ? name.GetString() : userRoot.GetProperty("login").GetString()
        };
    }

    private async Task<ExternalIdentity> GetMicrosoftIdentityAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var client = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new IdentityException("Invalid Microsoft access token.", StatusCodes.Status401Unauthorized);
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;

        var email = root.TryGetProperty("mail", out var mail) ? mail.GetString() : null;
        if (string.IsNullOrWhiteSpace(email) && root.TryGetProperty("userPrincipalName", out var upn))
        {
            email = upn.GetString();
        }

        return new ExternalIdentity
        {
            ProviderUserId = root.GetProperty("id").GetString() ?? string.Empty,
            Email = email,
            Name = root.TryGetProperty("displayName", out var displayName) ? displayName.GetString() : null
        };
    }

    private bool IsProviderEnabled(string provider)
    {
        var key = $"OAuth:{ToProviderConfigName(provider)}:Enabled";
        var overrideValue = _appSettingRepository.GetValue(key);
        if (bool.TryParse(overrideValue, out var enabledOverride))
        {
            return enabledOverride;
        }

        return _configuration.GetValue($"OAuth:{ToProviderConfigName(provider)}:Enabled", false);
    }

    private static string? NormalizeProvider(string provider)
    {
        var normalized = provider.Trim().ToLowerInvariant();
        return SupportedProviders.Contains(normalized) ? normalized : null;
    }

    private static string ToProviderConfigName(string provider)
    {
        return char.ToUpperInvariant(provider[0]) + provider[1..];
    }

    private static string BuildBaseUsername(string email, string? name)
    {
        var fromEmail = email.Split('@')[0];
        var candidate = string.IsNullOrWhiteSpace(name) ? fromEmail : name;
        candidate = candidate.Trim().ToLowerInvariant();
        var chars = candidate.Where(char.IsLetterOrDigit).ToArray();
        var compact = new string(chars);
        return string.IsNullOrWhiteSpace(compact) ? fromEmail.ToLowerInvariant() : compact;
    }

    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
    {
        var username = baseUsername;
        var suffix = 1;

        while (await _userManager.FindByNameAsync(username) is not null)
        {
            username = $"{baseUsername}{suffix}";
            suffix++;
        }

        return username;
    }

    private sealed class ExternalIdentity
    {
        public string ProviderUserId { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Name { get; init; }
    }
}
