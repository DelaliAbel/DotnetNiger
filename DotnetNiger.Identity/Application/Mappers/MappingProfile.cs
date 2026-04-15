// Mapping applicatif Identity: MappingProfile
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Application.Mappers;

/// <summary>
/// Mapping centralise entre entites domaine et DTOs.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Mappe un ApplicationUser vers un UserDto complet (roles + social links).
    /// </summary>
    public static async Task<UserResponse> ToUserDtoAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        DotnetNigerIdentityDbContext dbContext)
    {
        var roles = await userManager.GetRolesAsync(user);
        var socialLinks = await dbContext.SocialLinks
            .Where(link => link.UserId == user.Id)
            .Select(link => new SocialLinkResponse
            {
                Id = link.Id,
                Platform = link.Platform,
                Url = link.Url
            })
            .ToListAsync();

        return new UserResponse
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
    public static SocialLinkResponse ToSocialLinkDto(SocialLink link)
    {
        return new SocialLinkResponse
        {
            Id = link.Id,
            Platform = link.Platform,
            Url = link.Url
        };
    }
}
