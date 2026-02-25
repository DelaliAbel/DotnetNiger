namespace DotnetNiger.UI.Models.Requests;

public class SearchQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Type { get; set; } // Post, Event, Resource, Project
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
