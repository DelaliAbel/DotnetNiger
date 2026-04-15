using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface ICategoryPersistence : ICrudPersistence<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
}
