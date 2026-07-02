using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des profils membres : informations personnelles, compétences et certificats.</summary>
public class ProfileService(AppDbContext db) : IProfileService
{
    /// <summary>Profil complet d'un membre (compétences, liens sociaux).</summary>
    public async Task<ProfileResponse?> GetAsync(Guid userId)
    {
        var member = await db.Members.AsNoTracking()
            .Include(m => m.SocialLinks)
            .Include(m => m.Skills)
            .FirstOrDefaultAsync(m => m.Id == userId);

        if (member is null) return null;

        var profile = MapProfile(member);

        var cert = await db.Certificates
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.SubmissionDate)
            .Select(c => new CertificateInfo
            {
                Status = c.Status,
                CertificateType = c.CertificateType,
                SubmissionDate = c.SubmissionDate,
                ReviewedNotes = c.ReviewedNotes,
                ReviewedAt = c.ReviewedAt,
            })
            .FirstOrDefaultAsync();

        profile.Certificate = cert;
        return profile;
    }

    /// <summary>Met à jour le profil (le crée s'il n'existe pas). Gère un conflit d'insertion concurrent.</summary>
    public async Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request)
    {
        var member = await db.Members
            .Include(m => m.SocialLinks)
            .Include(m => m.Skills)
            .FirstOrDefaultAsync(m => m.Id == userId);

        if (member is null)
        {
            member = new Member
            {
                Id = userId,
                SocialLinks = new List<SocialLink>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Members.Add(member);
        }

        if (request.FullName is not null) member.FullName = request.FullName;
        if (request.PhoneNumber is not null) member.PhoneNumber = request.PhoneNumber;
        if (request.Bio is not null) member.Bio = request.Bio;
        if (request.AvatarUrl is not null) member.AvatarUrl = request.AvatarUrl;
        if (request.Country is not null) member.Country = request.Country;
        if (request.City is not null) member.City = request.City;
        if (request.IsTeamMember is not null) member.IsTeamMember = request.IsTeamMember.Value;
        if (request.Position is not null) member.Position = request.Position;
        if (request.Skills is not null)
        {
            db.MemberSkills.RemoveRange(member.Skills);
            member.Skills = request.Skills.Select(s => new MemberSkill
            {
                Id = Guid.NewGuid(),
                MemberId = userId,
                Name = s
            }).ToList();
        }
        member.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException) when (member.Id != Guid.Empty)
        {
            db.Entry(member).State = EntityState.Detached;
            member = await db.Members.Include(m => m.SocialLinks).Include(m => m.Skills).FirstOrDefaultAsync(m => m.Id == userId);
            if (member is null) throw;
            if (request.FullName is not null) member.FullName = request.FullName;
            if (request.PhoneNumber is not null) member.PhoneNumber = request.PhoneNumber;
            if (request.Bio is not null) member.Bio = request.Bio;
            if (request.AvatarUrl is not null) member.AvatarUrl = request.AvatarUrl;
            if (request.Country is not null) member.Country = request.Country;
            if (request.City is not null) member.City = request.City;
            if (request.IsTeamMember is not null) member.IsTeamMember = request.IsTeamMember.Value;
            if (request.Position is not null) member.Position = request.Position;
            if (request.Skills is not null)
            {
                db.MemberSkills.RemoveRange(member.Skills);
                member.Skills = request.Skills.Select(s => new MemberSkill
                {
                    Id = Guid.NewGuid(),
                    MemberId = userId,
                    Name = s
                }).ToList();
            }
            member.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return MapProfile(member);
    }

    /// <summary>Ajoute un lien social au profil d'un membre.</summary>
    public async Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == userId);
        if (member is null)
        {
            member = new Member { Id = userId, SocialLinks = new List<SocialLink>(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Members.Add(member);
        }

        var link = new SocialLink
        {
            Id = Guid.NewGuid(),
            MemberId = userId,
            Platform = request.Platform,
            Url = request.Url
        };

        db.SocialLinks.Add(link);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException) when (member.Id != Guid.Empty)
        {
            db.Entry(member).State = EntityState.Detached;
            member = await db.Members.FirstOrDefaultAsync(m => m.Id == userId);
            if (member is null) throw;
        }

        return new SocialLinkResponse { Id = link.Id, Platform = link.Platform, Url = link.Url };
    }

    /// <summary>Supprime un lien social du profil (vérifie l'appartenance).</summary>
    public async Task<bool> DeleteSocialLinkAsync(Guid userId, Guid socialLinkId)
    {
        var link = await db.SocialLinks.FirstOrDefaultAsync(s => s.Id == socialLinkId && s.MemberId == userId);
        if (link is null) return false;
        db.SocialLinks.Remove(link);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Soumet un certificat pour validation. Valide l'URL et le type avant enregistrement.</summary>
    public async Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request)
    {
        if (userId == Guid.Empty)
            throw new ValidationException(Messages.User.NotFound);

        if (!Uri.TryCreate(request.CertificateUrl, UriKind.Absolute, out _))
            throw new ValidationException(Messages.Certificate.InvalidUrl);

        if (string.IsNullOrWhiteSpace(request.CertificateType))
            throw new ValidationException(Messages.Certificate.TypeRequired);

        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == userId);
        if (member is null)
        {
            member = new Member { Id = userId, SocialLinks = new List<SocialLink>(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Members.Add(member);
        }

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

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException) when (member.Id != Guid.Empty)
        {
            db.Entry(member).State = EntityState.Detached;
            member = await db.Members.FirstOrDefaultAsync(m => m.Id == userId);
            if (member is null) throw;
            db.Certificates.Add(cert);
            await db.SaveChangesAsync();
        }

        return new CertificateResponse
        {
            Id = cert.Id,
            Status = cert.Status,
            SubmissionDate = cert.SubmissionDate,
            EstimatedWaitTime = Messages.Certificate.EstimatedWait,
            SupportEmail = Messages.Certificate.SupportEmail
        };
    }

    /// <summary>Approuve un certificat en attente.</summary>
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

    /// <summary>Rejette un certificat avec un motif.</summary>
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

    /// <summary>Liste des certificats avec filtre optionnel par statut.</summary>
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

    /// <summary>Vérifie si un utilisateur a déjà un certificat approuvé.</summary>
    public async Task<bool> HasApprovedCertificateAsync(Guid userId)
    {
        return await db.Certificates.AnyAsync(c => c.UserId == userId && c.Status == "Approved");
    }

    private static ProfileResponse MapProfile(Member m) => new()
    {
        Id = m.Id,
        FullName = m.FullName,
        Bio = m.Bio,
        AvatarUrl = m.AvatarUrl,
        PhoneNumber = m.PhoneNumber,
        Country = m.Country,
        City = m.City,
        IsTeamMember = m.IsTeamMember,
        Position = m.Position,
        CreatedAt = m.CreatedAt,
        SocialLinks = m.SocialLinks.Select(s => new SocialLinkResponse
        {
            Id = s.Id,
            Platform = s.Platform,
            Url = s.Url
        }).ToList(),
        Skills = m.Skills.Select(s => s.Name).ToList()
    };
}
