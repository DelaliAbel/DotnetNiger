using System.Net.Http.Json;
using System.Text.Json;
using DotnetNiger.Client.Models;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetNiger.Client.Services.Api;

public class ApiUploadService : ApiServiceBase, IUploadService
{
    private const long MaxFileSize = 3 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };
    private readonly ILogger<ApiUploadService> _logger;

    public ApiUploadService(HttpClient http, ILogger<ApiUploadService> logger) : base(http)
    {
        _logger = logger;
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

    public async Task<UploadResponse> UploadImageAsync(IBrowserFile file, UploadType type)
    {
        var bytes = await ReadFileBytesAsync(file);
        return await UploadImageBase64Async(Convert.ToBase64String(bytes), file.Name, type);
    }

    public async Task<UploadResponse> UploadImageBase64Async(string base64Content, string fileName, UploadType type)
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

        var result = await response.Content.ReadFromJsonAsync<UploadResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new UploadResponse
        {
            Success = false,
            Message = "Réponse inattendue du serveur."
        };
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        var response = await Http.DeleteAsync(BuildUrl(ApiEndpoints.Upload, new Dictionary<string, string?> { ["path"] = imageUrl }));
        return response.IsSuccessStatusCode;
    }

    public Task<string?> ResolveImageUrlAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.FromResult<string?>(null);

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            return Task.FromResult<string?>(imageUrl);

        var baseUri = Http.BaseAddress?.ToString().TrimEnd('/');
        return Task.FromResult<string?>($"{baseUri}{imageUrl}");
    }

    public string GetFolderPath(UploadType type) => type switch
    {
        UploadType.User => "/uploads/users",
        UploadType.Event => "/uploads/events",
        UploadType.Blog => "/uploads/blog",
        _ => "/uploads"
    };
}
