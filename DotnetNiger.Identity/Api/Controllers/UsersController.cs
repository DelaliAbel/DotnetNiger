// Controleur API Identity: UsersController
using System.Security.Claims;
using System.Text.Json;
using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/me")]
[Authorize]
// Endpoints pour le profil et les operations du compte.
public class UsersController : ApiControllerBase
{
    // Endpoints proteges pour le profil utilisateur.
    private readonly IUserService _userService;
    private readonly IFileUploadService _fileUploadService;
    private readonly FileUploadOptions _fileUploadOptions;
    private readonly IAvatarMetadataService _avatarMetadataService;
    private readonly ILoginHistoryService _loginHistoryService;
    private readonly ISocialLinkService _socialLinkService;
    private readonly IAccountDeletionService _accountDeletionService;
    private readonly IFeatureToggleService _featureToggleService;
    private readonly DotnetNigerIdentityDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IFileUploadService fileUploadService,
        IOptions<FileUploadOptions> fileUploadOptions,
        IAvatarMetadataService avatarMetadataService,
        ILoginHistoryService loginHistoryService,
        ISocialLinkService socialLinkService,
        IAccountDeletionService accountDeletionService,
        IFeatureToggleService featureToggleService,
        DotnetNigerIdentityDbContext dbContext,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _fileUploadService = fileUploadService;
        _fileUploadOptions = fileUploadOptions.Value;
        _avatarMetadataService = avatarMetadataService;
        _loginHistoryService = loginHistoryService;
        _socialLinkService = socialLinkService;
        _accountDeletionService = accountDeletionService;
        _featureToggleService = featureToggleService;
        _dbContext = dbContext;
        _logger = logger;
    }

    // Factorized user validation
    private Guid RequireUserId()
    {
        return RequireAuthenticatedUserId();
    }

    // Factorized avatar validation
    private void ValidateAvatar(IFormFile avatar)
    {
        if (avatar == null || avatar.Length <= 0)
        {
            throw new IdentityException("Aucun fichier avatar n'a été fourni. Veuillez sélectionner une image.", 400);
        }
        if (avatar.Length > _fileUploadOptions.MaxAvatarBytes)
        {
            throw new IdentityException($"L'image est trop volumineuse ({avatar.Length} octets). Taille maximale autorisée : {_fileUploadOptions.MaxAvatarBytes} octets.", 400);
        }
        if (!_fileUploadOptions.AllowedAvatarContentTypes.Contains(avatar.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new IdentityException($"Le type de fichier '{avatar.ContentType}' n'est pas autorisé. Types acceptés : {string.Join(", ", _fileUploadOptions.AllowedAvatarContentTypes)}.", 400);
        }
        var extension = GetAvatarExtension(avatar.ContentType, avatar.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new IdentityException("L'extension de l'image est invalide. Extensions acceptées : .jpg, .png, .webp.", 400);
        }
        if (!_fileUploadOptions.AllowedAvatarExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new IdentityException($"L'extension '{extension}' n'est pas autorisée. Extensions acceptées : {string.Join(", ", _fileUploadOptions.AllowedAvatarExtensions)}.", 400);
        }
    }


    [HttpGet]
    public async Task<IActionResult> Me()
    {
        var userId = RequireUserId();
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null)
        {
            _logger.LogWarning("Profil utilisateur introuvable pour {UserId}", userId);
            return NotFoundProblem("Profil utilisateur introuvable.");
        }
        return Success(profile);
    }


    [HttpPut]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userId = RequireUserId();
        var profile = await _userService.UpdateProfileAsync(userId, request);
        if (profile == null)
        {
            _logger.LogWarning("Échec de la mise à jour du profil pour {UserId}", userId);
            throw new IdentityException("Impossible de mettre a jour le profil utilisateur.", 500);
        }
        _logger.LogInformation("User {UserId} updated profile.", userId);
        return Success(profile, "Profile updated successfully.");
    }

    [HttpGet("avatar")]
    [ProducesResponseType(typeof(AvatarInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvatar()
    {
        var userId = RequireUserId();
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null)
        {
            _logger.LogWarning("Profil utilisateur introuvable pour {UserId}", userId);
            return NotFoundProblem("Profil utilisateur introuvable.");
        }
        var metadata = await _avatarMetadataService.GetMetadataAsync(profile.AvatarUrl);
        return Success(metadata);
    }


    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = RequireUserId();
        try
        {
            await _userService.ChangePasswordAsync(userId, request);
            _logger.LogInformation("User {UserId} changed password.", userId);
            return SuccessMessage("Password changed successfully.");
        }
        catch (IdentityException ex)
        {
            _logger.LogWarning(ex, "Erreur lors du changement de mot de passe pour {UserId}", userId);
            throw;
        }
    }


    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
    {
        if (!_featureToggleService.IsAvatarUploadEnabled())
        {
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Avatar upload is currently disabled.", Status = 503 });
        }

        var userId = RequireUserId();
        try
        {
            ValidateAvatar(avatar);
            var currentProfile = await _userService.GetProfileAsync(userId);
            var previousAvatarUrl = currentProfile?.AvatarUrl;
            var extension = GetAvatarExtension(avatar.ContentType, avatar.FileName);
            var fileName = $"avatars/{userId}/{Guid.NewGuid():N}{extension}";
            await using var stream = avatar.OpenReadStream();
            var relativeUrl = await _fileUploadService.UploadAsync(stream, fileName, avatar.ContentType);
            var absoluteUrl = BuildAbsoluteUrl(relativeUrl);
            var profile = await _userService.UpdateAvatarAsync(userId, absoluteUrl);
            _logger.LogInformation("User {UserId} uploaded avatar.", userId);
            if (!string.IsNullOrWhiteSpace(previousAvatarUrl) && !string.Equals(previousAvatarUrl, absoluteUrl, StringComparison.OrdinalIgnoreCase))
            {
                await TryDeleteAsync(previousAvatarUrl, "upload");
            }
            return Success(profile, "Avatar uploaded successfully.");
        }
        catch (IdentityException ex)
        {
            _logger.LogWarning(ex, "Erreur lors de l'upload d'avatar pour {UserId}", userId);
            throw;
        }
    }


    [HttpDelete("avatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAvatar()
    {
        if (!_featureToggleService.IsAvatarUploadEnabled())
        {
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Avatar management is currently disabled.", Status = 503 });
        }

        var userId = RequireUserId();
        var currentProfile = await _userService.GetProfileAsync(userId);
        if (currentProfile == null)
        {
            _logger.LogWarning("Profil utilisateur introuvable pour {UserId}", userId);
            return NotFoundProblem("Profil utilisateur introuvable.");
        }
        if (string.IsNullOrWhiteSpace(currentProfile.AvatarUrl))
        {
            return SuccessMessage("No avatar to delete.");
        }
        try
        {
            await _userService.ClearAvatarAsync(userId);
            await TryDeleteAsync(currentProfile.AvatarUrl, "delete");
            _logger.LogInformation("User {UserId} deleted avatar.", userId);
            return SuccessMessage("Avatar deleted successfully.");
        }
        catch (IdentityException ex)
        {
            _logger.LogWarning(ex, "Erreur lors de la suppression d'avatar pour {UserId}", userId);
            throw;
        }
    }


    [HttpGet("login-history")]
    public async Task<IActionResult> GetMyLoginHistory([FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var userId = RequireUserId();
        var history = await _loginHistoryService.GetUserHistoryAsync(userId, skip, take);
        return Success(history);
    }


    [HttpPost("export-data/request")]
    public async Task<IActionResult> RequestExport()
    {
        if (!_featureToggleService.IsProfileDataExportEnabled())
        {
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Profile data export is currently disabled.", Status = 503 });
        }

        var userId = RequireUserId();
        var requestId = Guid.NewGuid();

        var profile = await _userService.GetProfileAsync(userId);
        var logins = await _dbContext.LoginHistories.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.LoginAt)
            .Take(500)
            .ToListAsync();
        var socialLinks = await _socialLinkService.GetForUserAsync(userId);

        var exportPayload = new
        {
            requestId,
            generatedAt = DateTime.UtcNow,
            user = profile,
            loginHistory = logins,
            socialLinks
        };

        var exportDir = Path.Combine(AppContext.BaseDirectory, "uploads", "exports");
        Directory.CreateDirectory(exportDir);
        var filePath = Path.Combine(exportDir, $"export-{userId:N}-{requestId:N}.json");
        await System.IO.File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(exportPayload, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        return Success(new { requestId, status = "completed", downloadUrl = $"/me/export-data/download/{requestId}" }, "Export generated successfully.");
    }


    [HttpGet("export-data/download/{requestId:guid}")]
    public IActionResult DownloadExport(Guid requestId)
    {
        var userId = RequireUserId();
        var filePath = Path.Combine(AppContext.BaseDirectory, "uploads", "exports", $"export-{userId:N}-{requestId:N}.json");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFoundProblem("Export file not found.");
        }

        return PhysicalFile(filePath, "application/json", $"export-{requestId:N}.json", enableRangeProcessing: true);
    }

    [HttpPost("account-deletion/request")]
    public async Task<IActionResult> RequestAccountDeletion([FromBody] RequestAccountDeletionRequest request)
    {
        var userId = RequireUserId();
        var deletionRequest = await _accountDeletionService.RequestDeletionAsync(userId, request.Reason);
        return Success(deletionRequest, "Account deletion request submitted.");
    }

    [HttpDelete("account-deletion/request")]
    public async Task<IActionResult> CancelAccountDeletionRequest()
    {
        var userId = RequireUserId();
        await _accountDeletionService.CancelRequestAsync(userId);
        return SuccessMessage("Account deletion request cancelled.");
    }

    [HttpGet("account-deletion/status")]
    public async Task<IActionResult> GetAccountDeletionStatus()
    {
        var userId = RequireUserId();
        var request = await _accountDeletionService.GetLatestForUserAsync(userId);
        if (request is null)
        {
            return SuccessMessage("No account deletion request found.");
        }

        return Success(request);
    }

    private string BuildAbsoluteUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var relative = url.StartsWith('/') ? url : "/" + url;
        return $"{baseUrl}{relative}";
    }

    private static string GetAvatarExtension(string contentType, string fileName)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(fileExtension))
        {
            return fileExtension;
        }

        var extension = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => string.Empty
        };

        return extension;
    }

    private async Task TryDeleteAsync(string fileUrl, string context)
    {
        try
        {
            await _fileUploadService.DeleteAsync(fileUrl);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Avatar cleanup failed on {Context}", context);
        }
    }
}
