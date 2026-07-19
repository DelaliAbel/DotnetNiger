namespace DotnetNiger.Domain.DTOs.Requests;

public class UploadBase64Request
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Blog";
}
