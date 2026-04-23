namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface ICommunityProvisioningClient
{
    Task ProvisionPendingMemberAsync(Guid userId, string fullName, CancellationToken ct = default);
}
