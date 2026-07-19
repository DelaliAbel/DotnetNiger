using System.Net.Http.Json;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Api;

public class ApiTagService : ApiServiceBase, ITagService
{
    public ApiTagService(HttpClient http) : base(http)
    {
    }

    public async Task<List<TagDto>> GetAllAsync()
    {
        return await GetCollectionAsync<TagDto>(ApiEndpoints.Tags);
    }

    public async Task<TagDto?> GetByIdAsync(Guid id)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.Tags}/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<TagDto>(response);
    }

    public async Task<TagDto?> GetBySlugAsync(string slug)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.Tags}/{slug}");
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<TagDto>(response);
    }

    public async Task<TagDto?> CreateAsync(string name)
    {
        var content = JsonContent.Create(new { name });
        var response = await Http.PostAsync(ApiEndpoints.Tags, content);
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<TagDto>(response);
    }

    public async Task<TagDto?> UpdateAsync(Guid id, string name)
    {
        var content = JsonContent.Create(new { name });
        var response = await Http.PutAsync($"{ApiEndpoints.Tags}/{id}", content);
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<TagDto>(response);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"{ApiEndpoints.Tags}/{id}");
        return response.IsSuccessStatusCode;
    }
}
