using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

public interface IResourceCommandService
{
    Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid authorId, bool isAdmin, bool isCollaborator);
    Task<ResourceResponse?> UpdateAsync(Guid id, UpdateResourceRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    Task<ResourceResponse?> IncrementViewCountAsync(Guid id);
    Task SubmitForReviewAsync(Guid id);
    Task PublishAsync(Guid id);
}
