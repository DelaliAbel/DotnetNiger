using System.Security.Claims;
using DotnetNiger.Community.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des uploads d'images pour les articles, événements et profils.</summary>
[ApiController]
[Route("api/v1/upload")]
public class UploadController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    private const long MaxFileSize = 3 * 1024 * 1024;

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

    /// <summary>Upload un fichier image (multipart/form-data).</summary>
    /// <param name="file">Fichier image à uploader.</param>
    /// <param name="type">Type d'utilisation (Blog, Event, User).</param>
    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string type = "Blog")
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Success = false, Message = Messages.Upload.NoFile });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = $"{Messages.Upload.ExtensionNotAllowed}{ext}" });

        if (!AllowedMimeTypes.Contains(file.ContentType))
            return BadRequest(new { Success = false, Message = $"{Messages.Upload.MimeNotAllowed}{file.ContentType}" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { Success = false, Message = Messages.Upload.TooLarge });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var folder = type switch
        {
            "User" => "uploads/users",
            "Event" => "uploads/events",
            "Blog" => "uploads/blog",
            _ => "uploads"
        };

        var uploadsDir = Path.Combine(GetUploadsRoot(), folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = await BuildFileNameAsync(ext, type, uploadsDir);
        var filePath = Path.Combine(uploadsDir, fileName);

        ms.Position = 0;
        await using var fs = new FileStream(filePath, FileMode.Create);
        await ms.CopyToAsync(fs);

        return Ok(new
        {
            Success = true,
            ImageUrl = $"/{folder}/{fileName}",
            Message = Messages.Upload.Uploaded
        });
    }

    /// <summary>Upload une image encodée en base64.</summary>
    /// <param name="request">Fichier en base64 avec nom et type.</param>
    [HttpPost("base64")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadBase64([FromBody] UploadBase64Request request)
    {
        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = $"{Messages.Upload.ExtensionNotAllowed}{ext}" });

        var data = Convert.FromBase64String(request.Base64Content);

        if (data.Length > MaxFileSize)
            return BadRequest(new { Success = false, Message = Messages.Upload.TooLarge });

        using var ms = new MemoryStream(data);
        var mimeError = ValidateImageMime(ms);
        if (mimeError != null)
            return BadRequest(new { Success = false, Message = mimeError });

        var folder = request.Type switch
        {
            "User" => "uploads/users",
            "Event" => "uploads/events",
            "Blog" => "uploads/blog",
            _ => "uploads"
        };

        var uploadsDir = Path.Combine(GetUploadsRoot(), folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = await BuildFileNameAsync(ext, request.Type, uploadsDir);
        var filePath = Path.Combine(uploadsDir, fileName);

        await System.IO.File.WriteAllBytesAsync(filePath, data);

        return Ok(new
        {
            Success = true,
            ImageUrl = $"/{folder}/{fileName}",
            Message = Messages.Upload.Uploaded
        });
    }

    /// <summary>Supprime un fichier image uploadé.</summary>
    /// <param name="path">Chemin relatif du fichier.</param>
    [HttpDelete]
    public IActionResult Delete([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { Success = false, Message = Messages.Upload.PathRequired });

        var fullPath = Path.GetFullPath(Path.Combine(GetUploadsRoot(), path.TrimStart('/')));
        var uploadsDir = Path.GetFullPath(GetUploadsRoot());

        if (!fullPath.StartsWith(uploadsDir, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { Success = false, Message = Messages.Upload.PathNotAllowed });

        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { Success = false, Message = Messages.Upload.NotFound });

        System.IO.File.Delete(fullPath);
        return Ok(new { Success = true, Message = Messages.Upload.Deleted });
    }

    private Task<string> BuildFileNameAsync(string ext, string type, string uploadsDir)
    {
        if (type != "User")
            return Task.FromResult($"{Guid.NewGuid()}{ext}");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult($"{Guid.NewGuid()}{ext}");

        var name = $"{userId}{ext}";
        var existing = Directory.GetFiles(uploadsDir, $"{userId}.*");
        foreach (var f in existing)
        {
            if (!f.Equals(Path.Combine(uploadsDir, name), StringComparison.OrdinalIgnoreCase))
                System.IO.File.Delete(f);
        }
        return Task.FromResult(name);
    }

    private static string? ValidateImageMime(Stream stream)
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
}

public class UploadBase64Request
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Blog";
}
