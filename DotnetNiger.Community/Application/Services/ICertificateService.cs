using System.Security.Claims;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Interface pour la gestion des certificats membres.</summary>
public interface ICertificateService
{
    Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request);
    Task<CertificateResponse?> ApproveCertificateAsync(Guid certificateId);
    Task<CertificateResponse?> RejectCertificateAsync(Guid certificateId, string reason);
    Task<List<CertificateAdminDto>> GetCertificatesAsync(string? status = null);
    Task<bool> HasApprovedCertificateAsync(Guid userId);
    Task<(bool allowed, bool forceUnpublished, string? error)> CanCreateContentAsync(Guid userId, ClaimsPrincipal user);
}
