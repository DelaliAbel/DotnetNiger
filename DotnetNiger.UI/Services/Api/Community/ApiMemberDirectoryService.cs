using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Api;

public class ApiMemberDirectoryService : ApiServiceBase, IMemberDirectoryService
{
    public ApiMemberDirectoryService(HttpClient http) : base(http) { }

    public async Task<PaginatedDto<MemberDirectoryResponse>> GetAllAsync(string? query, string? country, int page = 1, int pageSize = 10)
    {
        var q = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(), ["pageSize"] = pageSize.ToString(),
            ["query"] = query, ["country"] = country
        };
        var url = BuildUrl(ApiEndpoints.Members, q);
        var response = await Http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return new PaginatedDto<MemberDirectoryResponse>();
        return await ApiResponseReader.ReadAsync<PaginatedDto<MemberDirectoryResponse>>(response)
               ?? new PaginatedDto<MemberDirectoryResponse>();
    }

    public async Task<MemberDirectoryResponse?> GetByIdAsync(Guid id)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.Members}/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<MemberDirectoryResponse>(response);
    }

    public async Task<List<TeamMemberResponse>> GetTeamMembersAsync()
    {
        var response = await Http.GetAsync(ApiEndpoints.MembersTeam);
        if (!response.IsSuccessStatusCode) return new();
        return await ApiResponseReader.ReadAsync<List<TeamMemberResponse>>(response) ?? new();
    }
}
