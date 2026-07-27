namespace DotnetNiger.Api.Services.General;

/// <summary>Service de traitement et stockage des images uploadées.
/// Crée automatiquement le dossier uploads/ et les sous-dossiers si inexistants.</summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly string _uploadPath;

    public ImageProcessingService()
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    /// <summary>Sauvegarde un fichier image dans le sous-dossier correspondant au type.</summary>
    public async Task<string> SaveAsync(Stream stream, string fileName, string type)
    {
        var ext = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid()}{ext}";
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

    /// <summary>Supprime un fichier image par son chemin relatif.</summary>
    public bool Delete(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        var uploadsRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"));
        var filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/')));
        if (!filePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)) return false;
        if (!File.Exists(filePath)) return false;
        File.Delete(filePath);
        return true;
    }
}
