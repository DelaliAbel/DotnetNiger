using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Contrat de repository pour le journal des actions admin.
public interface IAdminActionLogRepository : IRepository<AdminActionLog>, IAdminActionLogPersistence
{
}
