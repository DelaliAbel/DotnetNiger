using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Models.Requests;

namespace DotnetNiger.UI.Services.Contracts;

public interface IProfileService
{
      Task<UserDto> GetProfileAsync();
      Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request);
}