namespace DotnetNiger.Gateway.Services;

public interface IOpenGraphService
{
    Task<OGMetadata?> FetchMetadataAsync(string type, string slug);
}

public class OGMetadata
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
