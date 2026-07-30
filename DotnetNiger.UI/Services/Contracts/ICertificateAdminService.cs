using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface ICertificateAdminService
{
    Task<List<CertificateAdminDto>> GetAllAsync(string? status = null);
    Task<bool> ApproveAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default);
    Task<bool> RejectAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default);
}
