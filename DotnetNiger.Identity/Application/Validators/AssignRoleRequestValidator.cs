// Validation Identity: AssignRoleRequestValidator
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation de l'assignation d'un role a un utilisateur.
public static class AssignRoleRequestValidator
{
	public static void ValidateAndThrow(AssignRoleRequest request)
	{
		if (request is null)
		{
			throw new IdentityException("La requete est requise.", 400);
		}

		if (request.UserId == Guid.Empty)
		{
			throw new IdentityException("L'identifiant de l'utilisateur est requis.", 400);
		}

		if (string.IsNullOrWhiteSpace(request.RoleName))
		{
			throw new IdentityException("Le nom du role est requis.", 400);
		}
	}
}
