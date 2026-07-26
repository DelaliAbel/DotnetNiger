using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur d'upload et de gestion des images.</summary>
[ApiController]
[Route("api/upload")]
[Authorize]
[EnableRateLimiting("default")]
public class UploadController(IImageProcessingService imageService) : ControllerBase
{
    private const long MaxFileSize = 4 * 1024 * 1024;

    /// <summary>Upload un fichier image via un formulaire multipart.</summary>
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

    /// <summary>Upload une image encodée en Base64.</summary>
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

    /// <summary>Supprime une image par son chemin.</summary>
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
