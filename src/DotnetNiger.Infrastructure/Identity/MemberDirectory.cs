using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Résout les coordonnées d'un membre en lisant la table des comptes.
/// </summary>
/// <remarks>
/// La lecture passe par le dépôt plutôt que par le <c>UserManager</c> : prévenir vingt
/// inscrits ne doit pas construire vingt gestionnaire de mots de passe. Seules la projection
/// et l'existence sont lues, jamais les secrets.
/// </remarks>
public sealed class MemberDirectory(IUnitOfWork unitOfWork) : IMemberDirectory
{
    /// <inheritdoc />
    public async Task<MemberContact?> ObtenirAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var contact = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .Where(utilisateur => utilisateur.Id == memberId)
            .Select(utilisateur => new MemberContact(
                utilisateur.Email ?? string.Empty,
                utilisateur.FirstName + " " + utilisateur.LastName))
            .FirstOrDefaultAsync(cancellationToken);

        return contact is null
            ? null
            : contact with { FullName = NomLisible(contact) };
    }

    /// <summary>
    /// Choisit le nom à adopter : les prénoms et noms s'ils sont connus, l'adresse sinon.
    /// </summary>
    /// <param name="contact">Coordonnées brutes.</param>
    /// <returns>Nom à afficher dans le message.</returns>
    /// <remarks>
    /// Un compte créé par un fournisseur externe peut n'avoir ni prénom ni nom : l'adresse
    /// reste alors le seul moyen de s'adresser poliment à son titulaire.
    /// </remarks>
    private static string NomLisible(MemberContact contact)
    {
        var nom = contact.FullName.Trim();

        return nom.Length > 0 ? nom : contact.Email;
    }
}
