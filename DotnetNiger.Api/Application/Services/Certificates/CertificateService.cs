using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Application.Services.Certificates;

/// <summary>Service de gestion des certifications membres (soumission, approbation, rejet).</summary>
public class CertificateService : ICertificateService
{
    private readonly DotnetNigerDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CertificateService(DotnetNigerDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>Approuve un certificat et upgrade le rôle de l'utilisateur.</summary>
    public async Task<CertificateResponse?> ApproveCertificateAsync(Guid id)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var cert = await _db.Set<Certificate>()
                .Include(c => c.Member)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cert == null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            cert.Status = "Approved";
            cert.ReviewedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(cert.UserId.ToString());
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, RoleConstants.User))
                    await _userManager.RemoveFromRoleAsync(user, RoleConstants.User);

                if (!await _userManager.IsInRoleAsync(user, RoleConstants.Collaborator))
                    await _userManager.AddToRoleAsync(user, RoleConstants.Collaborator);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return MapToResponse(cert);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>Rejette un certificat avec une raison.</summary>
    public async Task<CertificateResponse?> RejectCertificateAsync(Guid id, string reason)
    {
        var cert = await _db.Set<Certificate>()
            .Include(c => c.Member)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cert == null) return null;
        cert.Status = "Rejected";
        cert.ReviewedNotes = reason;
        cert.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(cert);
    }

    /// <summary>Récupère les certificats filtrés par statut.</summary>
    public async Task<List<CertificateResponse>> GetCertificatesAsync(string? status)
    {
        var q = _db.Set<Certificate>()
            .Include(c => c.Member)
            .Include(c => c.User)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(c => c.Status == status);
        var certs = await q.OrderByDescending(c => c.SubmissionDate).ToListAsync();
        return certs.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère un certificat par identifiant.</summary>
    public async Task<CertificateResponse?> GetCertificateAsync(Guid id)
    {
        var cert = await _db.Set<Certificate>()
            .Include(c => c.Member)
            .Include(c => c.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        return cert == null ? null : MapToResponse(cert);
    }

    /// <summary>Récupère le dernier certificat d'un utilisateur.</summary>
    public async Task<CertificateResponse?> GetUserCertificateAsync(Guid userId)
    {
        var cert = await _db.Set<Certificate>()
            .Include(c => c.Member)
            .Include(c => c.User)
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.SubmissionDate)
            .FirstOrDefaultAsync();
        return cert == null ? null : MapToResponse(cert);
    }

    /// <summary>Soumet un nouveau certificat pour validation.</summary>
    public async Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new InvalidOperationException("User not found");

        var member = await _db.Set<Member>().FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null)
        {
            member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? ""
            };
            _db.Set<Member>().Add(member);
        }

        var cert = new Certificate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MemberId = member.Id,
            Member = member,
            User = user,
            CertificateUrl = request.CertificateUrl,
            CertificateType = request.CertificateType,
            Status = "Pending",
            SubmissionDate = DateTime.UtcNow
        };
        _db.Set<Certificate>().Add(cert);
        await _db.SaveChangesAsync();
        return MapToResponse(cert);
    }

    private static CertificateResponse MapToResponse(Certificate c)
    {
        var user = c.User;
        var member = c.Member;

        var userName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(userName)) userName = member?.FullName ?? string.Empty;
        var userEmail = user?.Email ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userEmail)) userEmail = member?.Email ?? string.Empty;
        var avatarUrl = user?.AvatarUrl ?? string.Empty;

        return new CertificateResponse
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = userName,
            UserEmail = userEmail,
            AvatarUrl = avatarUrl,
            CertificateUrl = c.CertificateUrl,
            CertificateType = c.CertificateType,
            Status = c.Status,
            SubmissionDate = c.SubmissionDate,
            ReviewedNotes = c.ReviewedNotes,
            ReviewedAt = c.ReviewedAt
        };
    }
}
