using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface ICertificateAdminService
{
    Task<List<CertificateAdminDto>> GetAllAsync(string? status = null);
    Task<bool> ApproveAsync(Guid id, string? notes = null);
    Task<bool> RejectAsync(Guid id, string? notes = null);
}
