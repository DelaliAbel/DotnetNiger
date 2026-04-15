using System.Linq.Expressions;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

/// <summary>
/// Interface générique pour les opérations CRUD
/// Hérite des abstractions d'Application pour assurer la cohérence architecturale
/// </summary>
public interface IRepository<TEntity> : ICrudPersistence<TEntity> where TEntity : class
{
}
