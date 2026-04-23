namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
    string HashToken(string token);
}
