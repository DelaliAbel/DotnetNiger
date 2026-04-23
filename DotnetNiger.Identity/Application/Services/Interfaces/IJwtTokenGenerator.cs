using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
}
