using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiPostService : ApiServiceBase, IPostService
{
    public ApiPostService(HttpClient http, ILogger<ApiPostService> logger) : base(http, logger) { }

    public async Task<List<PostDto>> GetAllPostsAsync()
    {
        return await GetCollectionAsync<PostDto>(ApiEndpoints.Posts);
    }

    public async Task<List<PostDto>> GetPublishedPostsAsync()
    {
        return await GetCollectionAsync<PostDto>(ApiEndpoints.Posts, new Dictionary<string, string?>
        {
            ["published"] = "true"
        });
    }

    public async Task<List<PostDto>> GetPostsByCategoryAsync(string categorySlug)
    {
        var posts = await GetCollectionAsync<PostDto>(ApiEndpoints.Posts, new Dictionary<string, string?>
        {
            ["category"] = categorySlug
        });

        return posts
            .Where(p => p.Categories.Any(c => c.Slug.Equals(categorySlug, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<List<PostDto>> GetPostsByTagAsync(string tagSlug)
    {
        var posts = await GetCollectionAsync<PostDto>(ApiEndpoints.Posts, new Dictionary<string, string?>
        {
            ["tag"] = tagSlug
        });

        return posts
            .Where(p => p.Tags.Any(t => t.Slug.Equals(tagSlug, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Posts}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PostDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<PostDto?> GetPostBySlugAsync(string slug)
    {
        var url = $"{ApiEndpoints.Posts}/{slug}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PostDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<PostDto?> CreatePostAsync(CreatePostRequest request, Guid currentId)
    {
        var url = ApiEndpoints.Posts;
        var response = await Http.PostAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ApiResponseReader.ReadErrorAsync(response);
            throw new InvalidOperationException(error ?? $"Erreur {(int)response.StatusCode} lors de la création de l'article.");
        }
        return await ApiResponseReader.ReadAsync<PostDto>(response);
    }

    public async Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostRequest request)
    {
        var url = $"{ApiEndpoints.Posts}/{id}";
        try
        {
            var response = await Http.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PostDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Posts}/{id}";
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

    public async Task<List<PostDto>> SearchPostsAsync(string query)
    {
        return await GetCollectionAsync<PostDto>(ApiEndpoints.Posts, new Dictionary<string, string?>
        {
            ["query"] = query
        });
    }

    public async Task<bool> PublishPostAsync(Guid postId)
    {
        var url = $"{ApiEndpoints.Posts}/{postId}/publish";
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

    public async Task<bool> UnPublishPostAsync(Guid postId)
    {
        var url = $"{ApiEndpoints.Posts}/{postId}/unpublish";
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

    public async Task IncrementViewCountAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Posts}/{id}/views";
        try
        {
            await Http.PostAsync(url, null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
        }
    }

    public async Task<List<PostDto>> GetAdminPostsAsync(string? status = null)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(status))
            query["published"] = status == "Published" ? "true" : status == "Draft" ? "false" : null;

        return await GetCollectionAsync<PostDto>($"{ApiEndpoints.Posts}/admin", query);
    }

    public async Task<List<PostDto>> GetMyPostsAsync()
    {
        var url = $"{ApiEndpoints.Posts}/mine";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<PostDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<PostDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<PostDto>();
        }
    }
}
