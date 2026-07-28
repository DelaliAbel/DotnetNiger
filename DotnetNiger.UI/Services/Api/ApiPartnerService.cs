using System.Net.Http.Json;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiPartnerService : ApiServiceBase, IPartnerService
{
    public ApiPartnerService(HttpClient http, ILogger<ApiPartnerService> logger) : base(http, logger) { }

    public async Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType)
    {
        var url = string.IsNullOrWhiteSpace(partnerType) ? ApiEndpoints.Partners : $"{ApiEndpoints.Partners}?partnerType={Uri.EscapeDataString(partnerType)}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<PartnerResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<List<PartnerResponse>> GetAllAsync()
    {
        var url = ApiEndpoints.Partners;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<PartnerResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<PartnerResponse?> GetByIdAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Partners}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PartnerResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<PartnerResponse?> CreateAsync(CreatePartnerRequest request)
    {
        var url = ApiEndpoints.Partners;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PartnerResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request)
    {
        var url = $"{ApiEndpoints.Partners}/{id}";
        try
        {
            var response = await Http.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PartnerResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Partners}/{id}";
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
}
