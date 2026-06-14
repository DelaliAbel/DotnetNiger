using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("api/v1/upload")]
public class UploadController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string type = "Blog")
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Success = false, Message = "Aucun fichier fourni." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = $"Extension non autorisée : {ext}" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { Success = false, Message = "Fichier trop volumineux (max 5 Mo)." });

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

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

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
        var folder = request.Type switch
        {
            "User" => "uploads/users",
            "Event" => "uploads/events",
            "Blog" => "uploads/blog",
            _ => "uploads"
        };

        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = $"Extension non autorisée : {ext}" });

        var uploadsDir = Path.Combine(env.WebRootPath, folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        var data = Convert.FromBase64String(request.Base64Content);
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
}

public class UploadBase64Request
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Blog";
}
