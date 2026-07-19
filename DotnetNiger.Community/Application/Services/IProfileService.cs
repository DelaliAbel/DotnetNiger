using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

public interface IProfileService
{
    Task<ProfileResponse?> GetAsync(Guid userId);
    Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request);
    Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request);
    Task<bool> DeleteSocialLinkAsync(Guid userId, Guid socialLinkId);
}
