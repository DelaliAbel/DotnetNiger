using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Community;

/// <summary>Service de gestion de l'annuaire des membres (profils, compétences, liens sociaux).</summary>
public class MemberDirectoryService : IMemberDirectoryService
{
    private readonly DotnetNigerDbContext _db;

    public MemberDirectoryService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère le profil membre d'un utilisateur.</summary>
    public async Task<MemberResponse> GetProfileAsync(Guid userId)
    {
        var member = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Profil membre non trouvé");

        return MapToResponse(member);
    }

    /// <summary>Met à jour ou crée le profil membre d'un utilisateur.</summary>
    public async Task<MemberResponse> UpdateProfileAsync(Guid userId, UpdateMemberRequest request)
    {
        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (member == null)
        {
            member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisplayName = request.DisplayName ?? "",
                Bio = request.Bio ?? string.Empty,
                Location = request.Location,
                WebsiteUrl = request.WebsiteUrl
            };
            _db.Members.Add(member);
        }
        else
        {
            if (request.DisplayName != null) member.DisplayName = request.DisplayName;
            if (request.Bio != null) member.Bio = request.Bio;
            if (request.Location != null) member.Location = request.Location;
            if (request.WebsiteUrl != null) member.WebsiteUrl = request.WebsiteUrl;
        }

        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(member);
    }

    /// <summary>Crée un nouveau profil membre pour un utilisateur.</summary>
    public async Task<MemberResponse> CreateProfileAsync(Guid userId, CreateMemberRequest request)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member != null)
            throw new InvalidOperationException("Le profil existe déjà pour cet utilisateur.");

        member = new Member
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = request.DisplayName,
            Bio = request.Bio ?? string.Empty,
            Location = request.Location,
            WebsiteUrl = request.WebsiteUrl
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return MapToResponse(member);
    }

    /// <summary>Supprime le profil membre d'un utilisateur.</summary>
    public async Task<bool> DeleteProfileAsync(Guid userId)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null) return false;

        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Récupère les membres paginés avec filtres.</summary>
    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? query, string? country, int page, int pageSize)
    {
        var queryable = _db.Members.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
            queryable = queryable.Where(m => m.DisplayName.Contains(query) || (m.Bio != null && m.Bio.Contains(query)));
        if (!string.IsNullOrWhiteSpace(country))
            queryable = queryable.Where(m => m.Location == country);

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .OrderBy(m => m.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<MemberResponse>(
            items.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    /// <summary>Récupère les membres de l'équipe.</summary>
    public async Task<List<MemberResponse>> GetTeamMembersAsync()
    {
        var members = await _db.Members
            .AsNoTracking()
            .Include(m => m.SocialLinks)
            .Include(m => m.User)
            .Where(m => m.IsTeamMember)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();

        return members.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère un membre par identifiant.</summary>
    public async Task<MemberResponse?> GetByIdAsync(Guid id)
    {
        var member = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
        return member == null ? null : MapToResponse(member);
    }

    /// <summary>Recherche des membres par nom ou bio.</summary>
    public async Task<PaginatedResponse<MemberResponse>> SearchAsync(string? query, int page, int pageSize)
    {
        var queryable = _db.Members.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
            queryable = queryable.Where(m => m.DisplayName.Contains(query) || (m.Bio != null && m.Bio.Contains(query)));

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .OrderBy(m => m.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<MemberResponse>(
            items.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    /// <summary>Ajoute une compétence au profil d'un membre.</summary>
    public async Task AddSkillAsync(Guid userId, string skillName)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Membre non trouvé");

        var existing = await _db.MemberSkills
            .AnyAsync(s => s.MemberId == member.Id && s.SkillName == skillName);
        if (!existing)
        {
            _db.MemberSkills.Add(new MemberSkill
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                SkillName = skillName
            });
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>Retire une compétence du profil d'un membre.</summary>
    public async Task RemoveSkillAsync(Guid userId, string skillName)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Membre non trouvé");

        var skill = await _db.MemberSkills
            .FirstOrDefaultAsync(s => s.MemberId == member.Id && s.SkillName == skillName);
        if (skill != null)
        {
            _db.MemberSkills.Remove(skill);
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>Ajoute un lien social au profil d'un membre.</summary>
    public async Task AddSocialLinkAsync(Guid userId, SocialLinkRequest request)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Membre non trouvé");

        _db.SocialLinks.Add(new SocialLink
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Platform = request.Platform,
            Url = request.Url
        });
        await _db.SaveChangesAsync();
    }

    /// <summary>Retire un lien social du profil d'un membre.</summary>
    public async Task RemoveSocialLinkAsync(Guid userId, Guid linkId)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Membre non trouvé");

        var link = await _db.SocialLinks
            .FirstOrDefaultAsync(l => l.Id == linkId && l.MemberId == member.Id);
        if (link != null)
        {
            _db.SocialLinks.Remove(link);
            await _db.SaveChangesAsync();
        }
    }

    private static MemberResponse MapToResponse(Member m) =>
        new()
        {
            Id = m.Id,
            UserId = m.UserId,
            DisplayName = m.DisplayName,
            FullName = !string.IsNullOrWhiteSpace(m.FullName) ? m.FullName : m.DisplayName,
            Bio = m.Bio,
            AvatarUrl = m.AvatarUrl,
            Position = m.Position,
            Location = m.Location,
            WebsiteUrl = m.WebsiteUrl,
            SocialLinks = m.SocialLinks?.Select(l => new SocialLinkResponse
            {
                Id = l.Id,
                Platform = l.Platform,
                Url = l.Url
            }).ToList() ?? [],
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
}
