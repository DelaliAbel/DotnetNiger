// Validation Identity: ChangePasswordRequestValidator
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation du changement de mot de passe.
public static class ChangePasswordRequestValidator
{
	public static void ValidateAndThrow(ChangePasswordRequest request)
	{
		if (request is null)
		{
			throw new IdentityException("La requete est requise.", 400);
		}

		if (string.IsNullOrWhiteSpace(request.CurrentPassword))
		{
			throw new IdentityException("Le mot de passe actuel est requis.", 400);
		}

		if (string.IsNullOrWhiteSpace(request.NewPassword))
		{
			throw new IdentityException("Le nouveau mot de passe est requis.", 400);
		}

		if (request.NewPassword.Length < 8)
		{
			throw new IdentityException("Le nouveau mot de passe doit contenir au moins 8 caracteres.", 400);
		}

		if (request.CurrentPassword == request.NewPassword)
		{
			throw new IdentityException("Le nouveau mot de passe doit etre different de l'actuel.", 400);
		}
	}
}
