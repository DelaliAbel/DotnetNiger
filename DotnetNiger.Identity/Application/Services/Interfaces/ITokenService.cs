using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface ITokenService
{
	Task<AuthDto> RefreshAsync(RefreshTokenRequest request);
}
