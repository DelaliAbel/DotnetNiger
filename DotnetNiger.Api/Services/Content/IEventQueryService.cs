using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

public interface IEventQueryService
{
    Task<PaginatedResponse<EventResponse>> GetAllAsync(
        string? status, string? query, string? location,
        string? category, string? tag, DateTime? from, DateTime? to,
        Guid? organizerId, int page, int pageSize);
    Task<EventResponse?> GetByIdAsync(Guid id);
    Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page, int pageSize);
    Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId);
}
