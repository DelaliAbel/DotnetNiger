using System.Net.Http.Json;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Api;

public class ApiCategoryService : ApiServiceBase, ICategoryService
{
    public ApiCategoryService(HttpClient http) : base(http)
    {
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await GetCollectionAsync<CategoryDto>(ApiEndpoints.Categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.Categories}/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<CategoryDto>(response);
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.Categories}/{slug}");
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<CategoryDto>(response);
    }

    public async Task<CategoryDto?> CreateAsync(string name, string description)
    {
        var content = JsonContent.Create(new { name, description });
        var response = await Http.PostAsync(ApiEndpoints.Categories, content);
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<CategoryDto>(response);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, string name, string description)
    {
        var content = JsonContent.Create(new { name, description });
        var response = await Http.PutAsync($"{ApiEndpoints.Categories}/{id}", content);
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<CategoryDto>(response);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"{ApiEndpoints.Categories}/{id}");
        return response.IsSuccessStatusCode;
    }
}
