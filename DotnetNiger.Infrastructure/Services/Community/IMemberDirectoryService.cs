using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Community;

public interface IMemberDirectoryService
{
    Task<MemberResponse> GetProfileAsync(Guid userId);
    Task<MemberResponse> UpdateProfileAsync(Guid userId, UpdateMemberRequest request);
    Task<MemberResponse> CreateProfileAsync(Guid userId, CreateMemberRequest request);
    Task<bool> DeleteProfileAsync(Guid userId);
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? query, string? country, int page, int pageSize);
    Task<List<MemberResponse>> GetTeamMembersAsync();
    Task<MemberResponse?> GetByIdAsync(Guid id);
    Task<PaginatedResponse<MemberResponse>> SearchAsync(string? query, int page, int pageSize);
    Task AddSkillAsync(Guid userId, string skillName);
    Task RemoveSkillAsync(Guid userId, string skillName);
    Task AddSocialLinkAsync(Guid userId, SocialLinkRequest request);
    Task RemoveSocialLinkAsync(Guid userId, Guid linkId);
}
