using System.Text.Json.Serialization;

namespace DotnetNiger.Common.DTOs.Responses;

/// <summary>Réponse paginée contenant une liste d'éléments.</summary>
public class PaginatedResponse<T>
{
    public PaginatedResponse() { }

    public PaginatedResponse(IList<T> items, int totalCount, int page, int pageSize)
    {
        Items = [.. items];
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    [JsonPropertyOrder(1)]
    public List<T> Items { get; set; } = [];

    [JsonPropertyOrder(2)]
    public int TotalCount { get; set; }

    [JsonPropertyOrder(3)]
    public int Page { get; set; }

    [JsonPropertyOrder(4)]
    public int PageSize { get; set; }

    [JsonPropertyOrder(5)]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));

    [JsonPropertyOrder(6)]
    public bool HasNextPage => Page < TotalPages;

    [JsonPropertyOrder(7)]
    public bool HasPreviousPage => Page > 1;
}
