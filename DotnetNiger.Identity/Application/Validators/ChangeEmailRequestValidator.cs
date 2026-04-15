// Validation Identity: ChangeEmailRequestValidator
using System.ComponentModel.DataAnnotations;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation du changement d'email.
public static class ChangeEmailRequestValidator
{
    public static void ValidateAndThrow(ChangeEmailRequest request)
    {
        if (request is null)
        {
            throw new IdentityException("La requete est requise.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.NewEmail))
        {
            throw new IdentityException("Le nouvel email est requis.", 400);
        }

        if (!new EmailAddressAttribute().IsValid(request.NewEmail))
        {
            throw new IdentityException("Le format de l'email est invalide.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            throw new IdentityException("Le mot de passe actuel est requis pour confirmer le changement.", 400);
        }
    }
}
