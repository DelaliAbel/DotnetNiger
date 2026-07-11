using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public partial class ProfileService(AppDbContext db, IIdentityApiClient identityApi) : IProfileService
{
    public async Task<ProfileResponse?> GetAsync(Guid userId)
    {
        var member = await db.Members.AsNoTracking()
            .Include(m => m.SocialLinks)
            .Include(m => m.Skills)
            .FirstOrDefaultAsync(m => m.Id == userId);

        if (member is null) return null;

        var profile = MapProfile(member);
        profile.Username = member.Email;

        var identityUser = await identityApi.GetUserAsync(userId);
        if (identityUser is not null) profile.IsActive = identityUser.IsActive;

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

    public async Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request)
    {
        var member = await EnsureMemberExistsAsync(userId);

        UpdateMemberFields(member, request);
        await SyncIdentityAsync(userId, request);
        if (request.Skills is not null)
            UpdateSkills(member, request.Skills);
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var profile = MapProfile(member);
        profile.Username = member.Email;

        var identityUser = await identityApi.GetUserAsync(userId);
        if (identityUser is not null) profile.IsActive = identityUser.IsActive;

        return profile;
    }

    private async Task<Member> EnsureMemberExistsAsync(Guid userId)
    {
        var member = await db.Members
            .Include(m => m.SocialLinks)
            .Include(m => m.Skills)
            .FirstOrDefaultAsync(m => m.Id == userId);

        if (member is null)
        {
            var identityUser = await identityApi.GetUserAsync(userId);
            member = new Member
            {
                Id = userId,
                Email = identityUser?.Email ?? string.Empty,
                FullName = identityUser?.FullName ?? string.Empty,
                Bio = identityUser?.Bio ?? string.Empty,
                AvatarUrl = identityUser?.AvatarUrl ?? string.Empty,
                Country = identityUser?.Country ?? string.Empty,
                City = identityUser?.City ?? string.Empty,
                PhoneNumber = identityUser?.PhoneNumber ?? string.Empty,
                SocialLinks = new List<SocialLink>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Members.Add(member);
        }

        return member;
    }

    private async Task SyncIdentityAsync(Guid userId, UpdateProfileRequest request)
    {
        if (request.FullName is not null)
        {
            var parts = request.FullName.Split(' ', 2, StringSplitOptions.TrimEntries);
            var firstName = parts[0];
            var lastName = parts.Length > 1 ? parts[1] : ".";
            await identityApi.UpdateUserProfileAsync(userId, firstName, lastName, null);
        }
        if (request.AvatarUrl is not null)
            await identityApi.UpdateUserProfileAsync(userId, null, null, request.AvatarUrl);
    }

    private static void UpdateMemberFields(Member member, UpdateProfileRequest request)
    {
        if (request.FullName is not null) member.FullName = request.FullName;
        if (request.PhoneNumber is not null) member.PhoneNumber = request.PhoneNumber;
        if (request.Bio is not null) member.Bio = request.Bio;
        if (request.AvatarUrl is not null) member.AvatarUrl = request.AvatarUrl;
        if (request.Country is not null) member.Country = request.Country;
        if (request.City is not null) member.City = request.City;
        if (request.IsTeamMember is not null) member.IsTeamMember = request.IsTeamMember.Value;
        if (request.Position is not null) member.Position = request.Position;
    }

    private void UpdateSkills(Member member, List<string> skills)
    {
        if (member.Skills.Count != 0)
            db.MemberSkills.RemoveRange(member.Skills);

        member.Skills = skills
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(s => new MemberSkill
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                Name = s
            }).ToList();
    }

    private static ProfileResponse MapProfile(Member m) => new()
    {
        Id = m.Id,
        Email = m.Email,
        FullName = m.FullName,
        Roles = string.IsNullOrEmpty(m.Roles) ? [] : m.Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList(),
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
