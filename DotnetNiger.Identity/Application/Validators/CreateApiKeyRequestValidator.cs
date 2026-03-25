// Validation Identity: CreateApiKeyRequestValidator
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Exceptions;

namespace DotnetNiger.Identity.Application.Validators;

// Validation de la creation d'une cle API.
public static class CreateApiKeyRequestValidator
{
	public static void ValidateAndThrow(CreateApiKeyRequest request)
	{
		if (request is null)
		{
			throw new IdentityException("La requete est requise.", 400);
		}

		if (string.IsNullOrWhiteSpace(request.Name))
		{
			throw new IdentityException("Le nom de la cle API est requis.", 400);
		}

		if (request.Name.Length > 100)
		{
			throw new IdentityException("Le nom de la cle API ne doit pas depasser 100 caracteres.", 400);
		}

		if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
		{
			throw new IdentityException("La date d'expiration doit etre dans le futur.", 400);
		}
	}
}
