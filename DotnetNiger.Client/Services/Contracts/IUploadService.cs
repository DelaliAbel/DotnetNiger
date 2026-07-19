using DotnetNiger.Client.Models;
using DotnetNiger.Client.Models.Responses;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetNiger.Client.Services.Contracts;

public interface IUploadService
{
    Task<UploadResponse> UploadImageAsync(IBrowserFile file, UploadType type);
    Task<UploadResponse> UploadImageBase64Async(string base64Content, string fileName, UploadType type);
    Task<bool> DeleteImageAsync(string imageUrl);
    Task<string?> ResolveImageUrlAsync(string imageUrl);
    string GetFolderPath(UploadType type);
}
