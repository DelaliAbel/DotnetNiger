using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface IEventRegistrationPersistence
{
    Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId);
    Task<EventRegistration?> GetUserRegistrationAsync(Guid eventId, Guid userId);
    Task<IEnumerable<EventRegistration>> GetEventRegistrationsAsync(Guid eventId);
    Task<IEnumerable<EventRegistration>> GetUserRegistrationsAsync(Guid userId);
    Task<EventRegistration> AddAsync(EventRegistration entity);
    Task<EventRegistration> UpdateAsync(EventRegistration entity);
}
