using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IAuthService
{
	Task<AuthDto> RegisterAsync(RegisterRequest request);
	Task<AuthDto> LoginAsync(LoginRequest request);
}
