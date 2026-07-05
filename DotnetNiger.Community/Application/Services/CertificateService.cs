using System.Security.Claims;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des certificats membres : soumission, approbation, rejet et consultation.</summary>
public class CertificateService(AppDbContext db) : ICertificateService
{
    public async Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request)
    {
        ValidateSubmission(userId, request);

        var member = await CreateMemberIfNotExists(userId);

        var cert = new Certificate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CertificateUrl = request.CertificateUrl,
            CertificateType = request.CertificateType,
            Status = Messages.Certificate.StatusPending,
            SubmissionDate = DateTime.UtcNow
        };

        db.Certificates.Add(cert);

        await SaveCertificateWithRetryAsync(member, cert, userId);

        return new CertificateResponse
        {
            Id = cert.Id,
            Status = cert.Status,
            SubmissionDate = cert.SubmissionDate,
            EstimatedWaitTime = Messages.Certificate.EstimatedWait,
            SupportEmail = Messages.Certificate.SupportEmail
        };
    }

    private static void ValidateSubmission(Guid userId, CertificateSubmissionRequest request)
    {
        if (userId == Guid.Empty)
            throw new ValidationException(Messages.User.NotFound);

        if (!Uri.TryCreate(request.CertificateUrl, UriKind.Absolute, out _))
            throw new ValidationException(Messages.Certificate.InvalidUrl);

        if (string.IsNullOrWhiteSpace(request.CertificateType))
            throw new ValidationException(Messages.Certificate.TypeRequired);
    }

    private async Task<Member> CreateMemberIfNotExists(Guid userId)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == userId);
        if (member is null)
        {
            member = new Member { Id = userId, SocialLinks = new List<SocialLink>(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Members.Add(member);
        }
        return member;
    }

    private async Task SaveCertificateWithRetryAsync(Member member, Certificate cert, Guid userId)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException) when (member.Id != Guid.Empty)
        {
            db.Entry(member).State = EntityState.Detached;
            member = (await db.Members.FirstOrDefaultAsync(m => m.Id == userId))!;
            if (member is null) throw;
            db.Certificates.Add(cert);
            await db.SaveChangesAsync();
        }
    }

    public async Task<CertificateResponse?> ApproveCertificateAsync(Guid certificateId)
    {
        var cert = await db.Certificates.FindAsync(certificateId);
        if (cert is null) return null;

        cert.Status = Messages.Certificate.StatusApproved;
        cert.ReviewedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new CertificateResponse
        {
            Id = cert.Id,
            Status = cert.Status,
            SubmissionDate = cert.SubmissionDate,
            EstimatedWaitTime = string.Empty,
            SupportEmail = Messages.Certificate.SupportEmail
        };
    }

    public async Task<CertificateResponse?> RejectCertificateAsync(Guid certificateId, string reason)
    {
        var cert = await db.Certificates.FindAsync(certificateId);
        if (cert is null) return null;

        cert.Status = Messages.Certificate.StatusRejected;
        cert.ReviewedNotes = reason;
        cert.ReviewedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new CertificateResponse
        {
            Id = cert.Id,
            Status = cert.Status,
            SubmissionDate = cert.SubmissionDate,
            EstimatedWaitTime = string.Empty,
            SupportEmail = Messages.Certificate.SupportEmail
        };
    }

    public async Task<List<CertificateAdminDto>> GetCertificatesAsync(string? status = null)
    {
        var query = db.Certificates
            .Include(c => c.Member)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        return await query
            .OrderByDescending(c => c.SubmissionDate)
            .Select(c => new CertificateAdminDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.Member.FullName ?? "",
                UserEmail = "",
                AvatarUrl = c.Member.AvatarUrl ?? "",
                CertificateUrl = c.CertificateUrl,
                CertificateType = c.CertificateType,
                Status = c.Status,
                SubmissionDate = c.SubmissionDate,
                ReviewedNotes = c.ReviewedNotes,
                ReviewedAt = c.ReviewedAt
            })
            .ToListAsync();
    }

    public async Task<bool> HasApprovedCertificateAsync(Guid userId)
    {
        return await db.Certificates.AnyAsync(c => c.UserId == userId && c.Status == "Approved");
    }

    public async Task<(bool allowed, bool forceUnpublished, string? error)> CanCreateContentAsync(Guid userId, ClaimsPrincipal user)
    {
        var isAdmin = user.IsInRole(RoleConstants.Admin) || user.IsInRole(RoleConstants.SuperAdmin);
        if (isAdmin) return (true, false, null);

        var isCollaborator = user.IsInRole(RoleConstants.Collaborator);
        if (!isCollaborator) return (false, false, null);

        var hasCert = await HasApprovedCertificateAsync(userId);
        if (!hasCert) return (false, false, Messages.Certificate.NeedValidCertificate);

        return (true, true, null);
    }
}
