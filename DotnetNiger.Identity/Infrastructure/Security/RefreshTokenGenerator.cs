// Composant securite Identity: RefreshTokenGenerator
using System.Security.Cryptography;
using System.Text;

namespace DotnetNiger.Identity.Infrastructure.Security;

public class RefreshTokenGenerator
{
	// Generation d'un refresh token aleatoire.
	public string GenerateToken()
	{
		var bytes = RandomNumberGenerator.GetBytes(64);
		return Convert.ToBase64String(bytes);
	}

	// Hash SHA256 du refresh token pour stockage securise.
	public static string HashToken(string token)
	{
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
		return Convert.ToBase64String(hash);
	}
}
