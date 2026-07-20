using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Content;

public interface IEventCommandService
{
    Task<EventResponse> CreateAsync(CreateEventRequest request, Guid organizerId, bool isAdmin, bool isCollaborator);
    Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    Task SubmitForReviewAsync(Guid id);
    Task PublishAsync(Guid id);
    Task CancelAsync(Guid id);
}
