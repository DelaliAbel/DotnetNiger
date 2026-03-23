// Validation Community: AddPartnerRequestValidator
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour ajout de partner.
/// </summary>
public static class AddPartnerRequestValidator
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 200;

    public static void ValidateAndThrow(AddPartnerRequest request)
    {
        if (request == null)
            throw new NotFoundException("Add partner request is required.");

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < MinNameLength)
            throw new NotFoundException($"Partner name must be at least {MinNameLength} characters.");

        if (request.Name.Length > MaxNameLength)
            throw new NotFoundException($"Partner name must not exceed {MaxNameLength} characters.");

        if (string.IsNullOrWhiteSpace(request.LogoUrl) || !IsValidUrl(request.LogoUrl))
            throw new NotFoundException("Logo URL is required and must be valid.");

        if (string.IsNullOrWhiteSpace(request.Website) || !IsValidUrl(request.Website))
            throw new NotFoundException("Website URL is required and must be valid.");

        if (string.IsNullOrWhiteSpace(request.PartnerType))
            throw new NotFoundException("Partner type is required.");

        if (request.PartnerType != "Partner" && request.PartnerType != "Sponsor")
            throw new NotFoundException("Partner type must be 'Partner' or 'Sponsor'.");
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
