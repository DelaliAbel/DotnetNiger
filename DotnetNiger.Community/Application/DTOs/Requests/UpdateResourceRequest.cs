namespace DotnetNiger.Community.Application.DTOs.Requests;

public class UpdateResourceRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? ResourceType { get; set; }
    public string? Level { get; set; }
    public bool? IsApproved { get; set; }
}
