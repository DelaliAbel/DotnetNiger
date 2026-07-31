using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiMemberDirectoryService : ApiServiceBase, IMemberDirectoryService
{
    public ApiMemberDirectoryService(HttpClient http, ILogger<ApiMemberDirectoryService> logger) : base(http, logger) { }

    public async Task<PaginatedDto<MemberDirectoryResponse>> GetAllAsync(string? query, string? country, int page = 1, int pageSize = 10)
    {
        var q = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(), ["pageSize"] = pageSize.ToString(),
            ["query"] = query, ["country"] = country
        };
        var url = BuildUrl(ApiEndpoints.Members, q);
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new PaginatedDto<MemberDirectoryResponse>();
            }
            return await ApiResponseReader.ReadAsync<PaginatedDto<MemberDirectoryResponse>>(response)
                   ?? new PaginatedDto<MemberDirectoryResponse>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new PaginatedDto<MemberDirectoryResponse>();
        }
    }

    public async Task<MemberDirectoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Members}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<MemberDirectoryResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<List<TeamMemberResponse>> GetTeamMembersAsync()
    {
        var url = ApiEndpoints.MembersTeam;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new();
            }
            return await ApiResponseReader.ReadAsync<List<TeamMemberResponse>>(response) ?? new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new();
        }
    }
}
