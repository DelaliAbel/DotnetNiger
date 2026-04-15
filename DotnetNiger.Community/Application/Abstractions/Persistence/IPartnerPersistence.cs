using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface IPartnerPersistence : ICrudPersistence<Partner>
{
    Task<Partner?> GetBySlugAsync(string slug);
}
