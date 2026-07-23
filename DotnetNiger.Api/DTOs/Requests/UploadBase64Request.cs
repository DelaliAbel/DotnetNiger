namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'upload d'un fichier en base64.</summary>
public class UploadBase64Request
{
    /// <summary>Nom du fichier.</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>Contenu du fichier encodé en base64.</summary>
    public string Base64Content { get; set; } = string.Empty;
    /// <summary>Type de contenu (Blog, Avatar, etc.).</summary>
    public string Type { get; set; } = "Blog";
}
