using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.User;

/// <summary>Service de gestion du profil public des utilisateurs et liens sociaux.</summary>
public class ProfileService : IProfileService
{
    private readonly DotnetNigerDbContext _db;

    public ProfileService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère le profil complet d'un utilisateur.</summary>
    public async Task<ProfileResponse?> GetAsync(Guid userId)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        var member = await _db.Members.AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId);

        var roles = await _db.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
            .ToListAsync();

        var skills = member != null
            ? await _db.MemberSkills.AsNoTracking()
                .Where(s => s.MemberId == member.Id)
                .Select(s => s.SkillName)
                .ToListAsync()
            : [];

        var socialLinks = member != null
            ? await _db.SocialLinks.AsNoTracking()
                .Where(l => l.MemberId == member.Id)
                .Select(l => new SocialLinkResponse { Id = l.Id, Platform = l.Platform, Url = l.Url })
                .ToListAsync()
            : [];

        var certificate = await _db.Certificates.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return new ProfileResponse
        {
            Id = user.Id,
            Email = user.Email ?? "",
            Username = user.UserName ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Bio = member?.Bio ?? "",
            AvatarUrl = user.AvatarUrl ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Country = member?.Country ?? "",
            City = member?.City ?? "",
            IsActive = user.IsActive,
            IsTeamMember = member?.IsTeamMember ?? false,
            Position = member?.Position ?? "",
            CreatedAt = user.CreatedAt,
            Skills = skills,
            Roles = roles,
            SocialLinks = socialLinks,
            Certificate = certificate != null ? new CertificateInfo
            {
                Status = certificate.Status,
                CertificateType = certificate.CertificateType,
                SubmissionDate = certificate.SubmissionDate,
                ReviewedNotes = certificate.ReviewedNotes,
                ReviewedAt = certificate.ReviewedAt
            } : null
        };
    }

    /// <summary>Met à jour le profil (nom complet, téléphone, avatar, bio, localisation).</summary>
    public async Task<ProfileResponse?> UpdateAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        if (request.FullName != null)
        {
            var parts = request.FullName.Split(' ', 2);
            user.FirstName = parts[0];
            if (parts.Length > 1) user.LastName = parts[1];
        }
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;

        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null)
        {
            member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                Bio = request.Bio ?? "",
                Country = request.Country ?? "",
                City = request.City ?? ""
            };
            _db.Members.Add(member);
        }
        else
        {
            if (request.Bio != null) member.Bio = request.Bio;
            if (request.Country != null) member.Country = request.Country;
            if (request.City != null) member.City = request.City;
            if (request.Position != null) member.Position = request.Position;
            if (request.IsTeamMember.HasValue) member.IsTeamMember = request.IsTeamMember.Value;
            member.UpdatedAt = DateTime.UtcNow;
        }

        if (request.Skills != null && member != null)
        {
            var existingSkills = await _db.MemberSkills
                .Where(s => s.MemberId == member.Id)
                .ToListAsync();
            _db.MemberSkills.RemoveRange(existingSkills);

            foreach (var skill in request.Skills.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                _db.MemberSkills.Add(new MemberSkill
                {
                    Id = Guid.NewGuid(),
                    MemberId = member.Id,
                    SkillName = skill.Trim()
                });
            }
        }

        await _db.SaveChangesAsync();
        return await GetAsync(userId);
    }

    /// <summary>Ajoute un lien social au profil du membre.</summary>
    public async Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Membre non trouvé");

        var link = new SocialLink
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Platform = request.Platform,
            Url = request.Url
        };
        _db.SocialLinks.Add(link);
        await _db.SaveChangesAsync();
        return new SocialLinkResponse { Id = link.Id, Platform = link.Platform, Url = link.Url };
    }

    /// <summary>Supprime un lien social du profil du membre.</summary>
    public async Task<bool> DeleteSocialLinkAsync(Guid userId, Guid linkId)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null) return false;
        var link = await _db.SocialLinks.FirstOrDefaultAsync(l => l.Id == linkId && l.MemberId == member.Id);
        if (link == null) return false;
        _db.SocialLinks.Remove(link);
        await _db.SaveChangesAsync();
        return true;
    }
}
