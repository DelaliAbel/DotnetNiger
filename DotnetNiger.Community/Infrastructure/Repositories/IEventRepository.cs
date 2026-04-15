using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IEventRepository : IRepository<Event>, IEventPersistence
{
    Task<Event?> GetBySlugAsync(string slug);
}
