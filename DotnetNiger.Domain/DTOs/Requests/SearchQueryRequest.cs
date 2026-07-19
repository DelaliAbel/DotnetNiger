namespace DotnetNiger.Domain.DTOs.Requests;

public class SearchQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
