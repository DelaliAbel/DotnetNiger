// Mapping applicatif Identity: MappingProfile
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Application.Mappers;

/// <summary>
/// Mapping centralise entre entites domaine et DTOs.
/// Remplace le code MapUserAsync duplique dans AuthService, UserService et TokenService.
/// </summary>
public static class UserMapper
{
	/// <summary>
	/// Mappe un ApplicationUser vers un UserDto complet (roles + social links).
	/// </summary>
	public static async Task<UserDto> ToUserDtoAsync(
		ApplicationUser user,
		UserManager<ApplicationUser> userManager,
		DotnetNigerIdentityDbContext dbContext)
	{
		var roles = await userManager.GetRolesAsync(user);
		var socialLinks = await dbContext.SocialLinks
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

	/// <summary>
	/// Mappe un SocialLink vers un SocialLinkDto.
	/// </summary>
	public static SocialLinkDto ToSocialLinkDto(SocialLink link)
	{
		return new SocialLinkDto
		{
			Id = link.Id,
			Platform = link.Platform,
			Url = link.Url
		};
	}
}
