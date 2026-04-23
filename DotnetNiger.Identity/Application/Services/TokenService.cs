// Service applicatif Identity: TokenService
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

public class TokenService : ITokenService
{
    // Rotation et reemission des tokens.
    private readonly DotnetNigerIdentityDbContext _dbContext;
    private readonly IRefreshTokenPersistence _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        DotnetNigerIdentityDbContext dbContext,
        IRefreshTokenPersistence refreshTokenRepository,
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TokenService> logger)
    {
        _dbContext = dbContext;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _jwtOptions = jwtOptions.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new IdentityException("Refresh token is required.", 400);
        }

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (storedToken == null || storedToken.RevokedAt != null)
        {
            throw new InvalidCredentialsException();
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}. Token expired at: {ExpiresAt}, Current UTC time: {UtcNow}",
                storedToken.UserId, storedToken.ExpiresAt, DateTime.UtcNow);
            throw new TokenExpiredException();
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found during token refresh. UserId: {UserId}", storedToken.UserId);
            throw new UserNotFoundException();
        }

        // Rotation du refresh token pour limiter la reutilisation.
        _logger.LogInformation("Initiating token rotation for user {UserId}. Revoking old token and issuing new one.", storedToken.UserId);
        await _refreshTokenRepository.RevokeAsync(storedToken);
        var newRefreshTokenValue = _refreshTokenGenerator.GenerateToken();
        var hashedToken = _refreshTokenGenerator.HashToken(newRefreshTokenValue);

        var httpContext = _httpContextAccessor.HttpContext;
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);

        var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
        var userDto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);

        _logger.LogInformation("Token successfully refreshed for user {UserId}. New token will expire in {Minutes} minutes.",
            user.Id, _jwtOptions.AccessTokenMinutes);

        return new AuthResponse
        {
            User = userDto,
            Token = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshTokenValue,
                ExpiresIn = _jwtOptions.AccessTokenMinutes * 60,
                TokenType = "Bearer"
            }
        };
    }

    public async Task LogoutAsync(Guid userId, RefreshTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new IdentityException("Refresh token is required.", 400);
        }

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null || storedToken.UserId != userId)
        {
            throw new InvalidCredentialsException();
        }

        if (storedToken.RevokedAt != null)
        {
            return;
        }

        await _refreshTokenRepository.RevokeAsync(storedToken);
    }
}
