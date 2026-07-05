using System.Security.Claims;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Common.Helpers;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Traitement et validation des images uploadées (SkiaSharp, sécurisation chemin, déduplication).</summary>
public class ImageProcessingService(IWebHostEnvironment env, IHttpContextAccessor httpContext, IConfiguration configuration) : IImageProcessingService
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    private const long MaxFileSize = 4 * 1024 * 1024;

    private string PublicBaseUrl => configuration["PublicBaseUrl"] ?? "";

    /// <summary>Valide et sauvegarde un fichier image sur le disque.</summary>
    public async Task<string> SaveAsync(Stream stream, string fileName, string type, string? userId = null)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(Messages.Upload.ExtensionNotAllowed + ext);

        if (stream.Length > MaxFileSize)
            throw new InvalidOperationException(Messages.Upload.TooLarge);

        stream.Position = 0;
        var mimeError = ValidateImage(stream);
        if (mimeError != null)
            throw new InvalidOperationException(mimeError);

        var folder = type switch
        {
            "User" => "uploads/users",
            "Event" => "uploads/events",
            "Blog" => "uploads/blog",
            _ => "uploads"
        };

        var uploadsDir = Path.Combine(GetUploadsRoot(), folder);
        Directory.CreateDirectory(uploadsDir);

        var finalName = await BuildFileNameAsync(ext, type, uploadsDir, userId);
        var filePath = Path.Combine(uploadsDir, finalName);

        stream.Position = 0;
        await using var fs = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fs);

        return ImageUrlHelper.ToAbsolute($"/{folder}/{finalName}", PublicBaseUrl);
    }

    /// <summary>Valide le format d'une image à partir de son flux binaire.</summary>
    public string? ValidateImage(Stream stream)
    {
        try
        {
            using var codec = SKCodec.Create(stream);
            if (codec == null)
                return Messages.Upload.InvalidImage;

            var detected = codec.EncodedFormat switch
            {
                SKEncodedImageFormat.Jpeg => "image/jpeg",
                SKEncodedImageFormat.Png => "image/png",
                SKEncodedImageFormat.Webp => "image/webp",
                SKEncodedImageFormat.Gif => "image/gif",
                _ => null
            };

            if (detected == null || !AllowedMimeTypes.Contains(detected))
                return Messages.Upload.TypeNotAllowed;
            return null;
        }
        catch
        {
            return Messages.Upload.InvalidImage;
        }
    }

    /// <summary>Supprime un fichier image par son chemin relatif.</summary>
    public bool Delete(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var fullPath = Path.GetFullPath(Path.Combine(GetUploadsRoot(), relativePath.TrimStart('/')));
        var uploadsDir = Path.GetFullPath(GetUploadsRoot());

        if (!fullPath.StartsWith(uploadsDir, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!File.Exists(fullPath))
            return false;

        File.Delete(fullPath);
        return true;
    }

    private string GetUploadsRoot()
    {
        var wwwroot = env.WebRootPath;
        if (string.IsNullOrEmpty(wwwroot))
        {
            wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(wwwroot);
        }
        return wwwroot;
    }

    private Task<string> BuildFileNameAsync(string ext, string type, string uploadsDir, string? userId)
    {
        userId ??= httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (type != "User" || string.IsNullOrWhiteSpace(userId))
            return Task.FromResult($"{Guid.NewGuid()}{ext}");

        var name = $"{userId}{ext}";
        var existing = Directory.GetFiles(uploadsDir, $"{userId}.*");
        foreach (var f in existing)
        {
            if (!f.Equals(Path.Combine(uploadsDir, name), StringComparison.OrdinalIgnoreCase))
                File.Delete(f);
        }
        return Task.FromResult(name);
    }
}
