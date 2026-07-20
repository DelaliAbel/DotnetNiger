using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Community;

public interface ICertificateService
{
    Task<CertificateResponse?> ApproveCertificateAsync(Guid id);
    Task<CertificateResponse?> RejectCertificateAsync(Guid id, string reason);
    Task<List<CertificateResponse>> GetCertificatesAsync(string? status);
    Task<CertificateResponse?> GetCertificateAsync(Guid id);
    Task<CertificateResponse?> GetUserCertificateAsync(Guid userId);
    Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request);
}
