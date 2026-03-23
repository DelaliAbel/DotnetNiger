// Validation Community: CreateProjectRequestValidator
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour creation de project.
/// </summary>
public static class CreateProjectRequestValidator
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 200;

    public static void ValidateAndThrow(CreateProjectRequest request)
    {
        if (request == null)
            throw new NotFoundException("Create project request is required.");

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < MinNameLength)
            throw new NotFoundException($"Project name must be at least {MinNameLength} characters.");

        if (request.Name.Length > MaxNameLength)
            throw new NotFoundException($"Project name must not exceed {MaxNameLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new NotFoundException("Description is required.");

        if (string.IsNullOrWhiteSpace(request.GitHubUrl) || !IsValidUrl(request.GitHubUrl))
            throw new NotFoundException("GitHub URL is required and must be valid.");

        if (!request.GitHubUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            throw new NotFoundException("URL must be a valid GitHub repository URL.");
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
