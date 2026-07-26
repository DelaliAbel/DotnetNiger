using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiCertificateAdminService : ApiServiceBase, ICertificateAdminService
{
    public ApiCertificateAdminService(HttpClient http, ILogger<ApiCertificateAdminService> logger) : base(http, logger) { }

    public async Task<List<CertificateAdminDto>> GetAllAsync(string? status = null)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(status))
            query["status"] = status;

        return await GetCollectionAsync<CertificateAdminDto>(ApiEndpoints.AdminCertificates, query);
    }

    public async Task<bool> ApproveAsync(Guid id, string? notes = null)
    {
        var url = $"{ApiEndpoints.AdminCertificates}/{id}/approve";
        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }

    public async Task<bool> RejectAsync(Guid id, string? notes = null)
    {
        var url = string.IsNullOrWhiteSpace(notes)
            ? $"{ApiEndpoints.AdminCertificates}/{id}/reject"
            : $"{ApiEndpoints.AdminCertificates}/{id}/reject?reason={Uri.EscapeDataString(notes)}";

        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }
}
