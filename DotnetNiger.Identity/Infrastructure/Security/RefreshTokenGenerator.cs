using System.Security.Cryptography;

namespace DotnetNiger.Identity.Infrastructure.Security;

public class RefreshTokenGenerator
{
	public string GenerateToken()
	{
		var bytes = RandomNumberGenerator.GetBytes(64);
		return Convert.ToBase64String(bytes);
	}
}
