using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("api/v1/upload")]
public class UploadController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    private const long MaxFileSize = 2 * 1024 * 1024;
    private const int MaxImageWidth = 4096;
    private const int MaxImageHeight = 4096;

    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string type = "Blog")
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Success = false, Message = "Aucun fichier fourni." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = $"Extension non autorisée : {ext}" });

        if (!AllowedMimeTypes.Contains(file.ContentType))
            return BadRequest(new { Success = false, Message = $"Type MIME non autorisé : {file.ContentType}" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { Success = false, Message = "Fichier trop volumineux (max 2 Mo)." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var dimError = ValidateImageDimensions(ms);
        if (dimError != null)
            return BadRequest(new { Success = false, Message = dimError });

        var folder = type switch
        {
            "User" => "uploads/users",
            "Event" => "uploads/events",
            "Blog" => "uploads/blog",
            _ => "uploads"
        };

        var uploadsDir = Path.Combine(env.WebRootPath, folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        ms.Position = 0;
        await using var fs = new FileStream(filePath, FileMode.Create);
        await ms.CopyToAsync(fs);

        return Ok(new
        {
            Success = true,
            ImageUrl = $"/{folder}/{fileName}",
            Message = "Image uploadée avec succès."
        });
    }

    [HttpPost("base64")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadBase64([FromBody] UploadBase64Request request)
    {
        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = $"Extension non autorisée : {ext}" });

        var data = Convert.FromBase64String(request.Base64Content);

        if (data.Length > MaxFileSize)
            return BadRequest(new { Success = false, Message = "Fichier trop volumineux (max 2 Mo)." });

        using var ms = new MemoryStream(data);
        var mimeError = ValidateImageMime(ms);
        if (mimeError != null)
            return BadRequest(new { Success = false, Message = mimeError });

        ms.Position = 0;
        var dimError = ValidateImageDimensions(ms);
        if (dimError != null)
            return BadRequest(new { Success = false, Message = dimError });

        var folder = request.Type switch
        {
            "User" => "uploads/users",
            "Event" => "uploads/events",
            "Blog" => "uploads/blog",
            _ => "uploads"
        };

        var uploadsDir = Path.Combine(env.WebRootPath, folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await System.IO.File.WriteAllBytesAsync(filePath, data);

        return Ok(new
        {
            Success = true,
            ImageUrl = $"/{folder}/{fileName}",
            Message = "Image uploadée avec succès."
        });
    }

    [HttpDelete]
    public IActionResult Delete([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { Success = false, Message = "Chemin requis." });

        var fullPath = Path.Combine(env.WebRootPath, path.TrimStart('/'));
        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { Success = false, Message = "Fichier introuvable." });

        System.IO.File.Delete(fullPath);
        return Ok(new { Success = true, Message = "Fichier supprimé." });
    }

    private static string? ValidateImageDimensions(Stream stream)
    {
        try
        {
            using var codec = SKCodec.Create(stream);
            if (codec == null)
                return "Format d'image non valide ou corrompu.";

            var info = codec.Info;
            if (info.Width > MaxImageWidth || info.Height > MaxImageHeight)
                return $"Dimensions trop grandes ({info.Width}x{info.Height}). Maximum : {MaxImageWidth}x{MaxImageHeight} px.";
            if (info.Width < 50 || info.Height < 50)
                return $"Dimensions trop petites ({info.Width}x{info.Height}). Minimum : 50x50 px.";
            return null;
        }
        catch
        {
            return "Format d'image non valide ou corrompu.";
        }
    }

    private static string? ValidateImageMime(Stream stream)
    {
        try
        {
            using var codec = SKCodec.Create(stream);
            if (codec == null)
                return "Format d'image non valide ou corrompu.";

            var detected = codec.EncodedFormat switch
            {
                SKEncodedImageFormat.Jpeg => "image/jpeg",
                SKEncodedImageFormat.Png => "image/png",
                SKEncodedImageFormat.Webp => "image/webp",
                SKEncodedImageFormat.Gif => "image/gif",
                _ => null
            };

            if (detected == null || !AllowedMimeTypes.Contains(detected))
                return "Type d'image non autorisé.";
            return null;
        }
        catch
        {
            return "Format d'image non valide ou corrompu.";
        }
    }
}

public class UploadBase64Request
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Blog";
}
