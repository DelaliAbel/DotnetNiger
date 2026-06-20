using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.Services;

public class ProfileService(AppDbContext db) : IProfileService
{
    public async Task<ProfileResponse?> GetAsync(Guid userId)
    {
        var member = await db.Members.AsNoTracking()
            .Include(m => m.SocialLinks)
            .Include(m => m.Skills)
            .FirstOrDefaultAsync(m => m.Id == userId);
        return member is null ? null : MapProfile(member);
    }

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

    public async Task<bool> DeleteSocialLinkAsync(Guid userId, Guid socialLinkId)
    {
        var link = await db.SocialLinks.FirstOrDefaultAsync(s => s.Id == socialLinkId && s.MemberId == userId);
        if (link is null) return false;
        db.SocialLinks.Remove(link);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<CertificateResponse> SubmitCertificateAsync(Guid userId, CertificateSubmissionRequest request)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("Utilisateur introuvable.");

        if (!Uri.TryCreate(request.CertificateUrl, UriKind.Absolute, out _))
            throw new ValidationException("URL de certification invalide.");

        if (string.IsNullOrWhiteSpace(request.CertificateType))
            throw new ValidationException("Veuillez sélectionner un type de certificat.");

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
            Status = "Pending",
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
            EstimatedWaitTime = "24-48 heures",
            SupportEmail = "support@dotnetniger.org"
        };
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
