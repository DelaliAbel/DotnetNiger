using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.User;

public interface IProfileService
{
    Task<ProfileResponse?> GetAsync(Guid userId);
    Task<ProfileResponse?> UpdateAsync(Guid userId, UpdateProfileRequest request);
    Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request);
    Task<bool> DeleteSocialLinkAsync(Guid userId, Guid linkId);
}
