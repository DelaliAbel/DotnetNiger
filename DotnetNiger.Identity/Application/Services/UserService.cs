using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Application.Services;

public class UserService : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly DotnetNigerIdentityDbContext _dbContext;

	public UserService(UserManager<ApplicationUser> userManager, DotnetNigerIdentityDbContext dbContext)
	{
		_userManager = userManager;
		_dbContext = dbContext;
	}

	public async Task<UserDto> GetProfileAsync(Guid userId)
	{
		var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
		if (user == null)
		{
			throw new UserNotFoundException();
		}

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
