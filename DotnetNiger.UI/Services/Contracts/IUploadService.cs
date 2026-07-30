using DotnetNiger.UI.Models;
using DotnetNiger.UI.Models.Responses;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IUploadService
{
    Task<UploadResponse> UploadImageAsync(IBrowserFile file, UploadType type, CancellationToken cancellationToken = default);
    Task<UploadResponse> UploadImageBase64Async(string base64Content, string fileName, UploadType type, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
    Task<string?> ResolveImageUrlAsync(string imageUrl, CancellationToken cancellationToken = default);
    string GetFolderPath(UploadType type);
}
