using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Mappers;

public interface ICommunityRequestMapper
{
    Post MapToPost(CreatePostRequest request, Guid authorId);
    void ApplyPostUpdates(Post post, UpdatePostRequest request);

    Project MapToProject(CreateProjectRequest request, Guid ownerId);
    void ApplyProjectUpdates(Project project, UpdateProjectRequest request);

    Resource MapToResource(CreateResourceRequest request, Guid createdBy);
    void ApplyResourceUpdates(Resource resource, UpdateResourceRequest request);

    Event MapToEvent(CreateEventRequest request, Guid createdBy);
    void ApplyEventUpdates(Event @event, UpdateEventRequest request);

    Category MapToCategory(CreateCategoryRequest request);
    void ApplyCategoryUpdates(Category category, UpdateCategoryRequest request);

    Partner MapToPartner(CreatePartnerRequest request);
    void ApplyPartnerUpdates(Partner partner, UpdatePartnerRequest request);

    Tag MapToTag(CreateTagRequest request);
    Comment MapToComment(CreateCommentRequest request, Guid postId, Guid userId);
    void ApplyCommentUpdates(Comment comment, UpdateCommentRequest request);
}
