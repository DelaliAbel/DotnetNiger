using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

public class AuthService : IAuthService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly DotnetNigerIdentityDbContext _dbContext;
	private readonly JwtTokenGenerator _jwtTokenGenerator;
	private readonly RefreshTokenGenerator _refreshTokenGenerator;
	private readonly JwtOptions _jwtOptions;

	public AuthService(
		UserManager<ApplicationUser> userManager,
		DotnetNigerIdentityDbContext dbContext,
		JwtTokenGenerator jwtTokenGenerator,
		RefreshTokenGenerator refreshTokenGenerator,
		IOptions<JwtOptions> jwtOptions)
	{
		_userManager = userManager;
		_dbContext = dbContext;
		_jwtTokenGenerator = jwtTokenGenerator;
		_refreshTokenGenerator = refreshTokenGenerator;
		_jwtOptions = jwtOptions.Value;
	}

	public async Task<AuthDto> RegisterAsync(RegisterRequest request)
	{
		var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
		if (existingByEmail != null)
		{
			throw new UserAlreadyExistsException("Email already in use.");
		}

		var existingByUsername = await _userManager.FindByNameAsync(request.Username);
		if (existingByUsername != null)
		{
			throw new UserAlreadyExistsException("Username already in use.");
		}

		var user = new ApplicationUser
		{
			UserName = request.Username,
			Email = request.Email,
			FullName = request.FullName,
			Country = request.Country,
			City = request.City,
			IsActive = true
		};

		var result = await _userManager.CreateAsync(user, request.Password);
		if (!result.Succeeded)
		{
			var message = string.Join(" ", result.Errors.Select(error => error.Description));
			throw new IdentityException(message, 400);
		}

		var tokenDto = await CreateTokenAsync(user);
		var userDto = await MapUserAsync(user);

		return new AuthDto
		{
			Success = true,
			Message = "Registration successful.",
			User = userDto,
			Token = tokenDto
		};
	}

	public async Task<AuthDto> LoginAsync(LoginRequest request)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);
		if (user == null)
		{
			throw new InvalidCredentialsException();
		}

		var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
		if (!passwordValid)
		{
			throw new InvalidCredentialsException();
		}

		user.LastLoginAt = DateTime.UtcNow;
		await _userManager.UpdateAsync(user);

		var tokenDto = await CreateTokenAsync(user);
		var userDto = await MapUserAsync(user);

		return new AuthDto
		{
			Success = true,
			Message = "Login successful.",
			User = userDto,
			Token = tokenDto
		};
	}

	private async Task<TokenDto> CreateTokenAsync(ApplicationUser user)
	{
		var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
		var refreshTokenValue = _refreshTokenGenerator.GenerateToken();
		var refreshToken = new RefreshToken
		{
			UserId = user.Id,
			Token = refreshTokenValue,
			ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
		};

		_dbContext.RefreshTokens.Add(refreshToken);
		await _dbContext.SaveChangesAsync();

		return new TokenDto
		{
			AccessToken = accessToken,
			RefreshToken = refreshTokenValue,
			ExpiresIn = _jwtOptions.AccessTokenMinutes * 60,
			TokenType = "Bearer"
		};
	}

	private async Task<UserDto> MapUserAsync(ApplicationUser user)
	{
		var roles = await _userManager.GetRolesAsync(user);
		var socialLinks = await _dbContext.SocialLinks
			.Where(link => link.UserId == user.Id)
			.Select(link => new SocialLinkDto
			{
				Id = link.Id,
				Platform = link.Platform,
				Url = link.Url
			})
			.ToListAsync();

		return new UserDto
		{
			Id = user.Id,
			Username = user.UserName ?? string.Empty,
			Email = user.Email ?? string.Empty,
			FullName = user.FullName,
			Bio = user.Bio,
			AvatarUrl = user.AvatarUrl,
			Country = user.Country ?? string.Empty,
			City = user.City ?? string.Empty,
			IsActive = user.IsActive,
			CreatedAt = user.CreatedAt,
			LastLoginAt = user.LastLoginAt,
			Roles = roles.ToList(),
			SocialLinks = socialLinks
		};
	}
}
