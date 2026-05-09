using DotnetNiger.Community.Dtos.Requests;
using DotnetNiger.Community.Dtos.Responses;

namespace DotnetNiger.Community.Services;

public interface IProfileService
{
    Task<ProfileResponse?> GetAsync(Guid userId);
    Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request);
    Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request);
    Task<bool> DeleteSocialLinkAsync(Guid userId, Guid socialLinkId);
}
