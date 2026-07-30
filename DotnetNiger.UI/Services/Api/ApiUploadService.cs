using System.Net.Http.Json;
using System.Text.Json;
using DotnetNiger.UI.Models;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiUploadService : ApiServiceBase, IUploadService
{
    private const long MaxFileSize = 3 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };
    private readonly ILogger<ApiUploadService> _logger;
    private readonly ApiBaseUrlProvider _baseUrlProvider;

    public ApiUploadService(HttpClient http, ILogger<ApiUploadService> logger, ApiBaseUrlProvider baseUrlProvider) : base(http, logger)
    {
        _logger = logger;
        _baseUrlProvider = baseUrlProvider;
    }

    private static async Task<byte[]> ReadFileBytesAsync(IBrowserFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.OpenReadStream(MaxFileSize).CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<string?> ReadErrorBodyAsync(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de lire le corps de la réponse d'erreur");
            return null;
        }
    }

    public async Task<UploadResponse> UploadImageAsync(IBrowserFile file, UploadType type, CancellationToken cancellationToken = default)
    {
        var bytes = await ReadFileBytesAsync(file);
        return await UploadImageBase64Async(Convert.ToBase64String(bytes), file.Name, type);
    }

    public async Task<UploadResponse> UploadImageBase64Async(string base64Content, string fileName, UploadType type, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);

        if (!AllowedExtensions.Contains(extension))
        {
            return new UploadResponse
            {
                Success = false,
                Message = $"Format non autorisé. Formats acceptés : {string.Join(", ", AllowedExtensions)}"
            };
        }

        var request = new
        {
            fileName,
            base64Content,
            type = type.ToString()
        };

        _logger.LogInformation("Upload base64 {Type} : {Name}", type, fileName);

        var response = await Http.PostAsJsonAsync(ApiEndpoints.UploadBase64, request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadErrorBodyAsync(response);
            _logger.LogWarning("Upload base64 échoué {StatusCode} : {Body}", response.StatusCode, body);
            return new UploadResponse
            {
                Success = false,
                Message = body ?? $"Erreur lors de l'upload : {response.StatusCode}"
            };
        }

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result.ValueKind == JsonValueKind.Object &&
            result.TryGetProperty("success", out var success) &&
            success.ValueKind == JsonValueKind.True &&
            result.TryGetProperty("data", out var data) &&
            data.ValueKind == JsonValueKind.Object)
        {
            var imageUrl = data.TryGetProperty("imageUrl", out var urlProp) && urlProp.ValueKind == JsonValueKind.String
                ? urlProp.GetString() ?? ""
                : "";
            var message = result.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String
                ? msg.GetString()
                : "Upload réussi";

            return new UploadResponse { Success = true, ImageUrl = imageUrl, Message = message ?? "Upload réussi", FileName = fileName };
        }

        return new UploadResponse { Success = false, Message = "Réponse inattendue du serveur." };
    }

    public async Task<bool> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync(BuildUrl(ApiEndpoints.Upload, new Dictionary<string, string?> { ["path"] = imageUrl }));
        return response.IsSuccessStatusCode;
    }

    public Task<string?> ResolveImageUrlAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.FromResult<string?>(null);

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            var apiHost = new Uri(_baseUrlProvider.BaseUrl);
            if (uri.Host == apiHost.Host && uri.Port == apiHost.Port)
                return Task.FromResult<string?>(imageUrl);

            return Task.FromResult<string?>($"{apiHost.Scheme}://{apiHost.Host}:{apiHost.Port}{uri.PathAndQuery}");
        }

        var baseUri = _baseUrlProvider.BaseUrl;
        return Task.FromResult<string?>($"{baseUri}{imageUrl}");
    }

    public string GetFolderPath(UploadType type) => type switch
    {
        UploadType.User => "/uploads/avatars",
        UploadType.Event => "/uploads/covers",
        UploadType.Blog => "/uploads/posts/blog",
        UploadType.Resource => "/uploads/resources",
        UploadType.Certificate => "/uploads/certificates",
        _ => "/uploads/files"
    };
}
