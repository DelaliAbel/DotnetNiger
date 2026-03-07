// Validation Identity: AddPermissionRequestValidator
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation de la creation d'une permission.
public static class AddPermissionRequestValidator
{
	public static void ValidateAndThrow(AddPermissionRequest request)
	{
		if (request is null)
		{
			throw new IdentityException("La requete est requise.", 400);
		}

		if (string.IsNullOrWhiteSpace(request.Name))
		{
			throw new IdentityException("Le nom de la permission est requis.", 400);
		}

		if (request.Name.Length > 100)
		{
			throw new IdentityException("Le nom de la permission ne doit pas depasser 100 caracteres.", 400);
		}
	}
}
