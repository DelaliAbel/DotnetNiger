// Service applicatif Identity: TokenService
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Mappers;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.Repositories;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

public class TokenService : ITokenService
{
	// Rotation et reemission des tokens.
	private readonly DotnetNigerIdentityDbContext _dbContext;
	private readonly IRefreshTokenRepository _refreshTokenRepository;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly JwtTokenGenerator _jwtTokenGenerator;
	private readonly RefreshTokenGenerator _refreshTokenGenerator;
	private readonly JwtOptions _jwtOptions;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public TokenService(
		DotnetNigerIdentityDbContext dbContext,
		IRefreshTokenRepository refreshTokenRepository,
		UserManager<ApplicationUser> userManager,
		JwtTokenGenerator jwtTokenGenerator,
		RefreshTokenGenerator refreshTokenGenerator,
		IOptions<JwtOptions> jwtOptions,
		IHttpContextAccessor httpContextAccessor)
	{
		_dbContext = dbContext;
		_refreshTokenRepository = refreshTokenRepository;
		_userManager = userManager;
		_jwtTokenGenerator = jwtTokenGenerator;
		_refreshTokenGenerator = refreshTokenGenerator;
		_jwtOptions = jwtOptions.Value;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<AuthDto> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
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
			throw new TokenExpiredException();
		}

		var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
		if (user == null)
		{
			throw new UserNotFoundException();
		}

		// Rotation du refresh token pour limiter la reutilisation.
		await _refreshTokenRepository.RevokeAsync(storedToken);
		var newRefreshTokenValue = _refreshTokenGenerator.GenerateToken();
		var hashedToken = RefreshTokenGenerator.HashToken(newRefreshTokenValue);

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

		return new AuthDto
		{
			Success = true,
			Message = "Token refreshed.",
			User = userDto,
			Token = new TokenDto
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
