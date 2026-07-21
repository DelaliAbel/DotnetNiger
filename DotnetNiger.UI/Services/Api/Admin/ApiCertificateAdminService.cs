using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Api;

public class ApiCertificateAdminService : ApiServiceBase, ICertificateAdminService
{
    public ApiCertificateAdminService(HttpClient http) : base(http)
    {
    }

    public async Task<List<CertificateAdminDto>> GetAllAsync(string? status = null)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(status))
            query["status"] = status;

        return await GetCollectionAsync<CertificateAdminDto>(ApiEndpoints.Certificates, query);
    }

    public async Task<bool> ApproveAsync(Guid id, string? notes = null)
    {
        var response = await Http.PatchAsync(
            $"{ApiEndpoints.Certificates}/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectAsync(Guid id, string? notes = null)
    {
        var url = string.IsNullOrWhiteSpace(notes)
            ? $"{ApiEndpoints.Certificates}/{id}/reject"
            : $"{ApiEndpoints.Certificates}/{id}/reject?reason={Uri.EscapeDataString(notes)}";

        var response = await Http.PatchAsync(url, null);
        return response.IsSuccessStatusCode;
    }
}
