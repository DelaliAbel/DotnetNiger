namespace DotnetNiger.Identity.Application.DTOs.Responses;

// Reponse paginee standard.
// Standard paginated response.
public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
