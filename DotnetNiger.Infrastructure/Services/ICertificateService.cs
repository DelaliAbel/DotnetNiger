using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface ICertificateService
{
    Task<CertificateResponse?> ApproveCertificateAsync(Guid id);
    Task<CertificateResponse?> RejectCertificateAsync(Guid id, string reason);
    Task<List<CertificateResponse>> GetCertificatesAsync(string? status);
    Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request);
}
