using Microsoft.EntityFrameworkCore;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Infrastructure.Data;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public class EventRegistrationRepository : BaseRepository<EventRegistration>, IEventRegistrationRepository, IEventRegistrationPersistence
{
    public EventRegistrationRepository(CommunityDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Vérifier si l'utilisateur est déjà enregistré à cet événement
    /// </summary>
    public async Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(r => r.EventId == eventId && r.UserId == userId && r.RegistrationStatus != "Cancelled");
    }

    /// <summary>
    /// Récupérer l'enregistrement d'un utilisateur pour un événement
    /// </summary>
    public async Task<EventRegistration?> GetUserRegistrationAsync(Guid eventId, Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
    }

    /// <summary>
    /// Récupérer tous les enregistrements d'un événement
    /// </summary>
    public async Task<IEnumerable<EventRegistration>> GetEventRegistrationsAsync(Guid eventId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.RegistrationStatus != "Cancelled")
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }

    /// <summary>
    /// Récupérer tous les enregistrements d'un utilisateur
    /// </summary>
    public async Task<IEnumerable<EventRegistration>> GetUserRegistrationsAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.RegistrationStatus != "Cancelled")
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }
}
