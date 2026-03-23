using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IPartnerRepository : IRepository<Partner>
{
    Task<Partner?> GetBySlugAsync(string slug);
}
