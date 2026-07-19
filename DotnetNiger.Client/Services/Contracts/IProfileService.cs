using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Models.Requests;

namespace DotnetNiger.Client.Services.Contracts;

public interface IProfileService
{
      Task<UserDto> GetProfileAsync();
      Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request);
      Task<List<SocialLinkDto>> GetSocialLinksAsync();
      Task<SocialLinkDto?> AddSocialLinkAsync(AddSocialLinkRequest request);
      Task<bool> DeleteSocialLinkAsync(Guid id);
}