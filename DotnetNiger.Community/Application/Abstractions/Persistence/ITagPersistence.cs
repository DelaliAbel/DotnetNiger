using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface ITagPersistence : ICrudPersistence<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
}
