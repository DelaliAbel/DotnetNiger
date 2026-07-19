using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Mock;

public partial class EventService
{
    // -- Inscriptions -------------------------------------------

    public async Task<EventRegistrationDto?> RegisterToEventAsync(RegisterEventRequest request, Guid userId, string userName)
    {
        var ev = _events.FirstOrDefault(e => e.Id == request.EventId);
        if (ev is null || ev.RegisteredCount >= ev.Capacity)
            return await Task.FromResult<EventRegistrationDto?>(null);

        var alreadyRegistered = _registrations.Any(r => r.EventId == request.EventId && r.UserId == userId);
        if (alreadyRegistered)
            return await Task.FromResult<EventRegistrationDto?>(null);

        var registration = new EventRegistrationDto
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            EventTitle = ev.Title,
            UserId = userId,
            UserName = userName,
            AvatarUrl = request.AvatarUrl,
            RegisteredAt = DateTime.Now,
            IsAttended = false,
            RegistrationStatus = "Confirmed"
        };

        _registrations.Add(registration);
        ev.RegisteredCount++;

        return await Task.FromResult<EventRegistrationDto?>(registration);
    }

    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var reg = _registrations.FirstOrDefault(r => r.EventId == eventId && r.UserId == userId);
        if (reg is null) return await Task.FromResult(false);

        _registrations.Remove(reg);
        var ev = _events.FirstOrDefault(e => e.Id == eventId);
        if (ev is not null) ev.RegisteredCount--;

        return await Task.FromResult(true);
    }

    public async Task<List<EventRegistrationDto>> GetRegistrationsByEventAsync(Guid eventId)
    {
        await Task.Delay(800);
        var registrations = _registrations
            .Where(r => r.EventId == eventId)
            .ToList();
        return await Task.FromResult(registrations);
    }
}
