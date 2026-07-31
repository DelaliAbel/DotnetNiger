using System.Net.Http.Json;
using DotnetNiger.UI.Configuration;
using System.Security.Claims;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Auth;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiCommentService : ApiServiceBase, ICommentService
{
    private readonly CustomAuthStateProvider _authProvider;
    private Guid? _currentUserId;

    public ApiCommentService(HttpClient http, ILogger<ApiCommentService> logger, CustomAuthStateProvider authProvider) : base(http, logger)
    {
        _authProvider = authProvider;
    }

    public async Task<Guid> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUserId.HasValue)
            return _currentUserId.Value;

        var token = await _authProvider.GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            return Guid.Empty;

        var claims = JwtParser.ParseClaimsFromJwt(token);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
        _currentUserId = userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var uid) ? uid : Guid.Empty;
        return _currentUserId.Value;
    }

    public async Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId)
    {
        var url = $"{ApiEndpoints.Comments}/post/{postId}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<List<CommentResponse>> GetCommentsByEventIdAsync(Guid eventId)
    {
        var url = $"{ApiEndpoints.Comments}/event/{eventId}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<CommentResponse?> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Comments}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<CommentResponse?> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.Comments;
        var response = await Http.PostAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ApiResponseReader.ReadErrorAsync(response);
            throw new InvalidOperationException(error ?? $"Erreur {(int)response.StatusCode} lors de la création du commentaire.");
        }
        return await ApiResponseReader.ReadAsync<CommentResponse>(response);
    }

    public async Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Comments}/{request.Id}";
        try
        {
            var response = await Http.PutAsJsonAsync(url, new { content = request.Content });
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<List<CommentResponse>> GetAllCommentsAsync()
    {
        var url = ApiEndpoints.Comments;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<CommentResponse?> ApproveCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Comments}/{id}/approve";
        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return null;
        }
    }

    public async Task<CommentResponse?> RejectCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Comments}/{id}/reject";
        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<CommentResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return null;
        }
    }


    public async Task<bool> DeleteCommentAsync(DeleteCommentRequest request, CancellationToken cancellationToken = default)
    {
        var url = request.DeleteAllReplies
            ? $"{ApiEndpoints.Comments}/{request.Id}?deleteAllReplies=true"
            : $"{ApiEndpoints.Comments}/{request.Id}";

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
