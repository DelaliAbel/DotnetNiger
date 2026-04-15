namespace DotnetNiger.Identity.Application.DTOs.Responses;

public class AvatarInfoResponse
{
    public string Url { get; set; } = string.Empty;
    public bool HasAvatar { get; set; }
    public string Provider { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public long? SizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
