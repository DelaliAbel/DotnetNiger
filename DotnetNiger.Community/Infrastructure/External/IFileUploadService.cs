namespace DotnetNiger.Community.Infrastructure.External;

public interface IFileUploadService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
