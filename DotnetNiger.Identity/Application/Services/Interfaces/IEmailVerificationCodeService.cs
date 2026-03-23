namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IEmailVerificationCodeService
{
    Task<string> CreateCodeAsync(string email, string identityToken, CancellationToken ct = default);
    Task<string?> ConsumeIdentityTokenAsync(string email, string code, CancellationToken ct = default);
}
