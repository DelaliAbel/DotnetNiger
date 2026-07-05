using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public partial class ProfileService
{
    public async Task<SocialLinkResponse> AddSocialLinkAsync(Guid userId, AddSocialLinkRequest request)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == userId);
        if (member is null)
        {
            var identityUser = await identityApi.GetUserAsync(userId);
            member = new Member
            {
                Id = userId,
                Email = identityUser?.Email ?? string.Empty,
                SocialLinks = new List<SocialLink>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
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
}
