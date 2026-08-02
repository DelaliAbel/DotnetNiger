namespace DotnetNiger.Api.Application.Services.ImageProcessing;

/// <summary>Service de traitement et stockage des images uploadées.
/// Crée automatiquement le dossier uploads/ et les sous-dossiers si inexistants.</summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly string _uploadPath;

    public ImageProcessingService(IOptions<UploadOptions> uploadOptions, IWebHostEnvironment environment)
    {
        var configured = uploadOptions.Value.Path;
        _uploadPath = Path.GetFullPath(
            !string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(environment.ContentRootPath, configured)
                : Path.Combine(environment.ContentRootPath, "wwwroot", "uploads"));
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    /// <summary>Sauvegarde un fichier image dans le sous-dossier correspondant au type.
    /// Valide le contenu (magic bytes) et n'autorise que les formats d'image raster.</summary>
    public async Task<string> SaveAsync(Stream stream, string fileName, string type)
    {
        var detectedExt = await DetectImageExtensionAsync(stream);
        if (detectedExt is null)
            throw new InvalidOperationException("Fichier image invalide ou format non supporté (JPG, PNG, GIF, WebP uniquement).");

        var safeName = $"{Guid.NewGuid()}{detectedExt}";
        var subFolder = type switch
        {
            "avatar" or "Avatar" or "User" => "avatars",
            "cover" or "Cover" or "Event" => "covers",
            "Blog" or "blog" => "posts/blog",
            "Resource" or "resource" => "resources",
            "Certificate" or "certificate" => "certificates",
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

    /// <summary>Détecte le format image à partir des magic bytes du fichier.</summary>
    private static async Task<string?> DetectImageExtensionAsync(Stream stream)
    {
        const int headerSize = 12;
        var header = new byte[headerSize];
        var read = await stream.ReadAsync(header.AsMemory(0, headerSize));
        if (read < headerSize) return null;
        if (stream.CanSeek) stream.Position = 0;

        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return ".jpg";
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return ".png";
        if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38) return ".gif";
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return ".webp";
        return null;
    }

    /// <summary>Supprime un fichier image par son chemin relatif.</summary>
    public bool Delete(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        var filePath = Path.GetFullPath(Path.Combine(_uploadPath, path.TrimStart('/')));
        if (!filePath.StartsWith(Path.GetFullPath(_uploadPath), StringComparison.OrdinalIgnoreCase)) return false;
        if (!File.Exists(filePath)) return false;
        File.Delete(filePath);
        return true;
    }
}
