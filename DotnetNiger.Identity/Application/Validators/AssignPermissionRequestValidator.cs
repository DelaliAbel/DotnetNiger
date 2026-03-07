// Validation Identity: AssignPermissionRequestValidator
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation de l'assignation d'une permission a un role.
public static class AssignPermissionRequestValidator
{
	public static void ValidateAndThrow(AssignPermissionRequest request)
	{
		if (request is null)
		{
			throw new IdentityException("La requete est requise.", 400);
		}

		if (request.RoleId == Guid.Empty)
		{
			throw new IdentityException("L'identifiant du role est requis.", 400);
		}

		if (request.PermissionId == Guid.Empty)
		{
			throw new IdentityException("L'identifiant de la permission est requis.", 400);
		}
	}
}
