using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockCertificateAdminService : ICertificateAdminService
{
    private readonly List<CertificateAdminDto> _certificates = new()
    {
        new CertificateAdminDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "Ahmed M.",
            UserEmail = "ahmed@example.com",
            CertificateType = "Participation",
            Status = "pending",
            SubmissionDate = DateTime.Now.AddDays(-2)
        },
        new CertificateAdminDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "Mariam O.",
            UserEmail = "mariam@example.com",
            CertificateType = "Completion",
            Status = "approved",
            SubmissionDate = DateTime.Now.AddDays(-5),
            ReviewedAt = DateTime.Now.AddDays(-3)
        }
    };

    public Task<List<CertificateAdminDto>> GetAllAsync(string? status = null)
    {
        var result = string.IsNullOrWhiteSpace(status)
            ? _certificates.ToList()
            : _certificates.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(result);
    }

    public Task<bool> ApproveAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default)
    {
        var cert = _certificates.FirstOrDefault(c => c.Id == id);
        if (cert is null) return Task.FromResult(false);
        cert.Status = "approved";
        cert.ReviewedAt = DateTime.Now;
        cert.ReviewedNotes = notes;
        return Task.FromResult(true);
    }

    public Task<bool> RejectAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default)
    {
        var cert = _certificates.FirstOrDefault(c => c.Id == id);
        if (cert is null) return Task.FromResult(false);
        cert.Status = "rejected";
        cert.ReviewedAt = DateTime.Now;
        cert.ReviewedNotes = notes;
        return Task.FromResult(true);
    }
}
