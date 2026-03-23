namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Response DTO for search results across multiple entity types.
/// </summary>
public class SearchResultsDto
{
    /// <summary>Gets or sets the search query that was executed.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of results across all types.</summary>
    public int TotalResults { get; set; }

    /// <summary>Gets or sets the posts matching the search query.</summary>
    public List<SearchPostResultDto> Posts { get; set; } = new();

    /// <summary>Gets or sets the events matching the search query.</summary>
    public List<SearchEventResultDto> Events { get; set; } = new();

    /// <summary>Gets or sets the projects matching the search query.</summary>
    public List<SearchProjectResultDto> Projects { get; set; } = new();

    /// <summary>Gets or sets the resources matching the search query.</summary>
    public List<SearchResourceResultDto> Resources { get; set; } = new();

    /// <summary>Gets or sets the timestamp when the search was executed.</summary>
    public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Minimal DTO for a post in search results.</summary>
public class SearchPostResultDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Minimal DTO for an event in search results.</summary>
public class SearchEventResultDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
}

/// <summary>Minimal DTO for a project in search results.</summary>
public class SearchProjectResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Minimal DTO for a resource in search results.</summary>
public class SearchResourceResultDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
