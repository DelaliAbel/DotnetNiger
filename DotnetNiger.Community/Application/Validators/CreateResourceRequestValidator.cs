// Validation Community: CreateResourceRequestValidator
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;
using DotnetNiger.Community.Domain.Constants;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour creation de resource.
/// </summary>
public static class CreateResourceRequestValidator
{
    private const int MinTitleLength = 3;
    private const int MaxTitleLength = 200;

    public static void ValidateAndThrow(CreateResourceRequest request)
    {
        if (request == null)
            throw new NotFoundException("Create resource request is required.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < MinTitleLength)
            throw new NotFoundException($"Title must be at least {MinTitleLength} characters.");

        if (request.Title.Length > MaxTitleLength)
            throw new NotFoundException($"Title must not exceed {MaxTitleLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new NotFoundException("Description is required.");

        if (string.IsNullOrWhiteSpace(request.Url) || !IsValidUrl(request.Url))
            throw new NotFoundException("URL is required and must be valid.");

        if (string.IsNullOrWhiteSpace(request.ResourceType))
            throw new NotFoundException("Resource type is required.");

        if (string.IsNullOrWhiteSpace(request.Level))
            throw new NotFoundException("Level is required.");

        if (!DomainConstants.ResourceLevels.IsValid(request.Level))
            throw new NotFoundException($"Level must be one of: {string.Join(", ", DomainConstants.ResourceLevels.All)}.");
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
