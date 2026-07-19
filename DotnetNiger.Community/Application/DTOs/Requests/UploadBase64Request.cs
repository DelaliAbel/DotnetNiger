namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête d'upload d'un fichier encodé en base64.</summary>
public class UploadBase64Request
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Blog";
}
