namespace DotnetNiger.Community.Infrastructure.External;

public class AzureBlobService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;

    public AzureBlobService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, safeFileName);

        await using var output = File.Create(filePath);
        await content.CopyToAsync(output, cancellationToken);

        return $"/uploads/{safeFileName}";
    }

    public Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(false);

        var normalized = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", normalized);

        if (!File.Exists(absolutePath))
            return Task.FromResult(false);

        File.Delete(absolutePath);
        return Task.FromResult(true);
    }
}
