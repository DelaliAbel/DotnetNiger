using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly DotnetNigerDbContext _db;

    public ProfileService(DotnetNigerDbContext db) => _db = db;

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

        return new ProfileResponse
        {
            Id = user.Id,
            Email = user.Email ?? "",
            Username = user.UserName ?? "",
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Bio = member?.Bio ?? "",
            AvatarUrl = user.AvatarUrl ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }

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

        await _db.SaveChangesAsync();
        return await GetAsync(userId);
    }

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
