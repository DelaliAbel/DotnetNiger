using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IPostRepository : IRepository<Post>, IPostPersistence
{
}
