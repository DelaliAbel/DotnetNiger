using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des certificats.</summary>
public interface ICertificateService
{
    /// <summary>Approuve un certificat.</summary>
    Task<CertificateResponse?> ApproveCertificateAsync(Guid id);
    /// <summary>Rejette un certificat.</summary>
    Task<CertificateResponse?> RejectCertificateAsync(Guid id, string reason);
    /// <summary>Récupère les certificats filtrés.</summary>
    Task<List<CertificateResponse>> GetCertificatesAsync(string? status);
    /// <summary>Récupère un certificat par identifiant.</summary>
    Task<CertificateResponse?> GetCertificateAsync(Guid id);
    /// <summary>Récupère le certificat d'un utilisateur.</summary>
    Task<CertificateResponse?> GetUserCertificateAsync(Guid userId);
    /// <summary>Soumet un certificat.</summary>
    Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request);
}
