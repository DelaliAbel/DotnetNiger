using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;

namespace DotnetNiger.UI.Services.Api;

public class ApiPostService : IPostService
{
    private readonly HttpClient _http;
    private const string PublicBase = "api/posts";
    private const string AdminBase = "api/community/admin/posts";

    public ApiPostService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PostDto>> GetAllPostsAsync()
    {
        return await _http.GetFromJsonAsync<List<PostDto>>(PublicBase) ?? new List<PostDto>();
    }

    public async Task<List<PostDto>> GetPublishedPostsAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p => p.PublishedAt != DateTime.MinValue).ToList();
    }

    public async Task<List<PostDto>> GetPostsByCategoryAsync(string categorySlug)
    {
        var posts = await GetAllPostsAsync();
        return posts
            .Where(p => p.Categories.Any(c => c.Slug.Equals(categorySlug, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<List<PostDto>> GetPostsByTagAsync(string tagSlug)
    {
        var posts = await GetAllPostsAsync();
        return posts
            .Where(p => p.Tags.Any(t => t.Slug.Equals(tagSlug, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<PostDto>($"{PublicBase}/{id}");
    }

    public async Task<PostDto?> GetPostBySlugAsync(string slug)
    {
        var posts = await GetAllPostsAsync();
        return posts.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PostDto> CreatePostAsync(CreatePostRequest request)
    {
        var response = await _http.PostAsJsonAsync(AdminBase, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostDto>()
               ?? throw new InvalidOperationException("La réponse API est vide pour la création du post.");
    }

    public async Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{AdminBase}/{id}", request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PostDto>();
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{AdminBase}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PostDto>> SearchPostsAsync(string query)
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p =>
                p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Excerpt.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.AuthorName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
