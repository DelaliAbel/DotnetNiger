// Validation Identity: AddRoleRequestValidator
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation de la creation d'un role.
public static class AddRoleRequestValidator
{
    public static void ValidateAndThrow(AddRoleRequest request)
    {
        if (request is null)
        {
            throw new IdentityException("La requete est requise.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new IdentityException("Le nom du role est requis.", 400);
        }

        if (request.Name.Length > 50)
        {
            throw new IdentityException("Le nom du role ne doit pas depasser 50 caracteres.", 400);
        }
    }
}
