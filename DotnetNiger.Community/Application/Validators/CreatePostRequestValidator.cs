// Validation Community: CreatePostRequestValidator
using System.ComponentModel.DataAnnotations;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;
using DotnetNiger.Community.Domain.Constants;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour creation de post.
/// </summary>
public static class CreatePostRequestValidator
{
    private const int MinTitleLength = 3;
    private const int MaxTitleLength = 200;
    private const int MaxExcerptLength = 500;

    public static void ValidateAndThrow(CreatePostRequest request)
    {
        if (request == null)
            throw new NotFoundException("Create post request is required.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < MinTitleLength)
            throw new NotFoundException($"Title must be at least {MinTitleLength} characters.");

        if (request.Title.Length > MaxTitleLength)
            throw new NotFoundException($"Title must not exceed {MaxTitleLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new NotFoundException("Content is required.");

        if (!string.IsNullOrWhiteSpace(request.Excerpt) && request.Excerpt.Length > MaxExcerptLength)
            throw new NotFoundException($"Excerpt must not exceed {MaxExcerptLength} characters.");

        if (string.IsNullOrWhiteSpace(request.PostType))
            throw new NotFoundException("Post type is required.");

        // Validate against DomainConstants
        if (!DomainConstants.PostTypes.IsValid(request.PostType))
            throw new NotFoundException($"Post type must be one of: {string.Join(", ", DomainConstants.PostTypes.All)}.");

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl) && !IsValidUrl(request.CoverImageUrl))
            throw new NotFoundException("Cover image URL is invalid.");
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
