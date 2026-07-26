using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiProfileService : ApiServiceBase, IProfileService
{
    public ApiProfileService(HttpClient http, ILogger<ApiProfileService> logger)
        : base(http, logger)
    {
    }

    public async Task<UserDto> GetProfileAsync()
    {
        var url = ApiEndpoints.Profile;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new UserDto();
            }
            return await ApiResponseReader.ReadAsync<UserDto>(response) ?? new UserDto();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new UserDto();
        }
    }

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var url = ApiEndpoints.Profile;
        try
        {
            var response = await Http.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return new UserDto();
            }
            var updated = await ApiResponseReader.ReadAsync<UserDto>(response);
            return updated ?? new UserDto();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return new UserDto();
        }
    }

    public async Task<List<SocialLinkDto>> GetSocialLinksAsync()
    {
        var url = ApiEndpoints.SocialLinks;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<SocialLinkDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<SocialLinkDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<SocialLinkDto>();
        }
    }

    public async Task<SocialLinkDto?> AddSocialLinkAsync(AddSocialLinkRequest request)
    {
        var url = ApiEndpoints.SocialLinks;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }

            if (response.Content.Headers.ContentLength is null or 0)
            {
                var links = await GetSocialLinksAsync();
                return links.LastOrDefault(link =>
                    link.Platform.Equals(request.Platform, StringComparison.OrdinalIgnoreCase) &&
                    link.Url.Equals(request.Url, StringComparison.OrdinalIgnoreCase));
            }

            return await ApiResponseReader.ReadAsync<SocialLinkDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeleteSocialLinkAsync(Guid id)
    {
        var url = $"{ApiEndpoints.SocialLinks}/{id}";
        try
        {
            var response = await Http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on DELETE {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on DELETE {Url}", url);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var url = ApiEndpoints.ProfileChangePassword;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> ChangeEmailAsync(ChangeEmailRequest request)
    {
        var url = ApiEndpoints.ProfileChangeEmail;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> ConfirmChangeEmailAsync(ConfirmChangeEmailRequest request)
    {
        var url = ApiEndpoints.ProfileConfirmChangeEmail;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> DeleteProfileAsync()
    {
        var url = ApiEndpoints.Profile;
        try
        {
            var response = await Http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on DELETE {Url}", (int)response.StatusCode, url);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on DELETE {Url}", url);
            return false;
        }
    }

    public async Task<bool> RequestDeletionAsync()
    {
        var url = ApiEndpoints.ProfileDeleteRequest;
        try
        {
            var response = await Http.PostAsJsonAsync(url, new { });
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> CancelDeletionAsync()
    {
        var url = ApiEndpoints.ProfileCancelDeletion;
        try
        {
            var response = await Http.PostAsJsonAsync(url, new { });
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }
}
