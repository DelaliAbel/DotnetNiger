using Asp.Versioning;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des uploads d'images pour les articles, événements et profils.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/upload")]
public class UploadController(IImageProcessingService imageService) : ControllerBase
{
    private const long MaxFileSize = 4 * 1024 * 1024;

    /// <summary>Upload un fichier image (multipart/form-data).</summary>
    /// <param name="file">Fichier image à uploader.</param>
    /// <param name="type">Type d'utilisation (Blog, Event, User).</param>
    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string type = "Blog")
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Success = false, Message = Messages.Upload.NoFile });

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var imageUrl = await imageService.SaveAsync(ms, file.FileName, type);
            return Ok(new { Success = true, ImageUrl = imageUrl, Message = Messages.Upload.Uploaded });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>Upload une image encodée en base64.</summary>
    /// <param name="request">Fichier en base64 avec nom et type.</param>
    [HttpPost("base64")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadBase64([FromBody] UploadBase64Request request)
    {
        byte[] data;
        try
        {
            data = Convert.FromBase64String(request.Base64Content);
        }
        catch (FormatException)
        {
            return BadRequest(new { Success = false, Message = Messages.Upload.InvalidImage });
        }

        if (data.Length > MaxFileSize)
            return BadRequest(new { Success = false, Message = Messages.Upload.TooLarge });

        try
        {
            using var ms = new MemoryStream(data);
            var imageUrl = await imageService.SaveAsync(ms, request.FileName, request.Type);
            return Ok(new { Success = true, ImageUrl = imageUrl, Message = Messages.Upload.Uploaded });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>Supprime un fichier image uploadé.</summary>
    /// <param name="path">Chemin relatif du fichier.</param>
    [HttpDelete]
    public IActionResult Delete([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { Success = false, Message = Messages.Upload.PathRequired });

        if (!imageService.Delete(path))
            return NotFound(new { Success = false, Message = Messages.Upload.NotFound });

        return Ok(new { Success = true, Message = Messages.Upload.Deleted });
    }
}
