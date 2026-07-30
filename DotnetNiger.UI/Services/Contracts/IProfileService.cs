using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Models.Requests;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IProfileService
{
      Task<UserDto> GetProfileAsync(CancellationToken cancellationToken = default);
      Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
      Task<List<SocialLinkDto>> GetSocialLinksAsync();
      Task<SocialLinkDto?> AddSocialLinkAsync(AddSocialLinkRequest request, CancellationToken cancellationToken = default);
      Task<bool> DeleteSocialLinkAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
      Task<bool> ChangeEmailAsync(ChangeEmailRequest request, CancellationToken cancellationToken = default);
      Task<bool> ConfirmChangeEmailAsync(ConfirmChangeEmailRequest request, CancellationToken cancellationToken = default);
      Task<bool> DeleteProfileAsync(CancellationToken cancellationToken = default);
      Task<bool> RequestDeletionAsync(CancellationToken cancellationToken = default);
      Task<bool> CancelDeletionAsync(CancellationToken cancellationToken = default);
}