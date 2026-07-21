namespace DotnetNiger.UI.Models.Responses;

public class EventMediaDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
