namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Response for search results across multiple entity types.
/// </summary>
public class SearchResultsResponse
{
    /// <summary>Gets or sets the search query that was executed.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of results across all types.</summary>
    public int TotalResults { get; set; }

    /// <summary>Gets or sets the posts matching the search query.</summary>
    public List<SearchPostResultResponse> Posts { get; set; } = new();

    /// <summary>Gets or sets the events matching the search query.</summary>
    public List<SearchEventResultResponse> Events { get; set; } = new();

    /// <summary>Gets or sets the projects matching the search query.</summary>
    public List<SearchProjectResultResponse> Projects { get; set; } = new();

    /// <summary>Gets or sets the resources matching the search query.</summary>
    public List<SearchResourceResultResponse> Resources { get; set; } = new();

    /// <summary>Gets or sets the timestamp when the search was executed.</summary>
    public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Minimal search item for a post.</summary>
public class SearchPostResultResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Minimal search item for an event.</summary>
public class SearchEventResultResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
}

/// <summary>Minimal search item for a project.</summary>
public class SearchProjectResultResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Minimal search item for a resource.</summary>
public class SearchResourceResultResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
