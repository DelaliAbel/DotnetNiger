// Validation Community: SearchQueryValidator
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour recherche.
/// </summary>
public static class SearchQueryValidator
{
    private const int MinQueryLength = 2;
    private const int MaxQueryLength = 100;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public static void ValidateAndThrow(SearchQueryRequest request)
    {
        if (request == null)
            throw new NotFoundException("Search query request is required.");

        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < MinQueryLength)
            throw new NotFoundException($"Query must be at least {MinQueryLength} characters.");

        if (request.Query.Length > MaxQueryLength)
            throw new NotFoundException($"Query must not exceed {MaxQueryLength} characters.");

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var validTypes = new[] { "Post", "Event", "Resource", "Project" };
            if (!validTypes.Contains(request.Type, StringComparer.OrdinalIgnoreCase))
                throw new NotFoundException("Type must be 'Post', 'Event', 'Resource', or 'Project'.");
        }

        if (request.Page < 1)
            throw new NotFoundException("Page must be at least 1.");

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
            throw new NotFoundException($"Page size must be between 1 and {MaxPageSize}.");
    }
}
