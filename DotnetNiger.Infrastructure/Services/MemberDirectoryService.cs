using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class MemberDirectoryService : IMemberDirectoryService
{
    private readonly DotnetNigerDbContext _db;

    public MemberDirectoryService(DotnetNigerDbContext db) => _db = db;

    public async Task<MemberResponse> GetProfileAsync(Guid userId)
    {
        var member = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Profil membre non trouvé");

        return MapToResponse(member);
    }

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
                Bio = request.Bio,
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
            Bio = request.Bio,
            Location = request.Location,
            WebsiteUrl = request.WebsiteUrl
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return MapToResponse(member);
    }

    public async Task<bool> DeleteProfileAsync(Guid userId)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null) return false;

        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }

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

    public async Task<List<MemberResponse>> GetTeamMembersAsync()
    {
        var members = await _db.Members
            .AsNoTracking()
            .Where(m => m.IsTeamMember)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();

        return members.Select(MapToResponse).ToList();
    }

    public async Task<MemberResponse?> GetByIdAsync(Guid id)
    {
        var member = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
        return member == null ? null : MapToResponse(member);
    }

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
        new(m.Id, m.UserId, m.DisplayName, m.Bio, m.Location, m.WebsiteUrl,
            m.CreatedAt, m.UpdatedAt);
}
