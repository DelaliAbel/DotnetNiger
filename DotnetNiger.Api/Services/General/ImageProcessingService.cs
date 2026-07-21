using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.General;

public class ImageProcessingService : IImageProcessingService
{
    private readonly string _uploadPath;

    public ImageProcessingService()
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string type)
    {
        var ext = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid()}{ext}";
        var subFolder = type switch
        {
            "avatar" => "avatars",
            "cover" => "covers",
            "image" => "images",
            _ => "files"
        };
        var dir = Path.Combine(_uploadPath, subFolder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, safeName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return $"/uploads/{subFolder}/{safeName}";
    }

    public bool Delete(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/'));
        if (!File.Exists(filePath)) return false;
        File.Delete(filePath);
        return true;
    }
}
