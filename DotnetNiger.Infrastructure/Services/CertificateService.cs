using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    private readonly DotnetNigerDbContext _db;

    public CertificateService(DotnetNigerDbContext db) => _db = db;

    public async Task<CertificateResponse?> ApproveCertificateAsync(Guid id)
    {
        var cert = await _db.Set<Certificate>().FindAsync(id);
        if (cert == null) return null;
        cert.Status = "Approved";
        cert.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(cert);
    }

    public async Task<CertificateResponse?> RejectCertificateAsync(Guid id, string reason)
    {
        var cert = await _db.Set<Certificate>().FindAsync(id);
        if (cert == null) return null;
        cert.Status = "Rejected";
        cert.ReviewedNotes = reason;
        cert.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(cert);
    }

    public async Task<List<CertificateResponse>> GetCertificatesAsync(string? status)
    {
        var q = _db.Set<Certificate>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(c => c.Status == status);
        var certs = await q.OrderByDescending(c => c.SubmissionDate).ToListAsync();
        return certs.Select(MapToResponse).ToList();
    }

    public async Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request)
    {
        var cert = new Certificate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CertificateUrl = request.CertificateUrl,
            CertificateType = request.CertificateType,
            Status = "Pending",
            SubmissionDate = DateTime.UtcNow
        };
        _db.Set<Certificate>().Add(cert);
        await _db.SaveChangesAsync();
        return MapToResponse(cert);
    }

    private static CertificateResponse MapToResponse(Certificate c) => new()
    {
        Id = c.Id, Status = c.Status, SubmissionDate = c.SubmissionDate
    };
}
