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

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, MemberProfile>> ObtenirPlusieursAsync(
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        if (memberIds.Count == 0)
        {
            return new Dictionary<Guid, MemberProfile>();
        }

        var membres = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .Where(utilisateur => memberIds.Contains(utilisateur.Id))
            .Select(utilisateur => new
            {
                utilisateur.Id,
                utilisateur.Email,
                utilisateur.FirstName,
                utilisateur.LastName,
                utilisateur.Profession,
            })
            .ToListAsync(cancellationToken);

        return membres.ToDictionary(
            membre => membre.Id,
            membre => new MemberProfile(
                NomLisible(new MemberContact(membre.Email ?? string.Empty, $"{membre.FirstName} {membre.LastName}")),
                membre.Profession));
    }

    /// <inheritdoc />
    public Task<int> CompterMembresAsync(CancellationToken cancellationToken) =>
        unitOfWork.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .CountAsync(utilisateur => utilisateur.IsActive, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TeamMember>> ListerEquipeAsync(CancellationToken cancellationToken)
    {
        var membres = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .Where(utilisateur => utilisateur.IsActive && utilisateur.IsTeam)
            .OrderBy(utilisateur => utilisateur.LastName)
            .ThenBy(utilisateur => utilisateur.FirstName)
            .Select(utilisateur => new TeamMember(
                NomLisible(new MemberContact(
                    utilisateur.Email ?? string.Empty,
                    $"{utilisateur.FirstName} {utilisateur.LastName}")),
                utilisateur.Profession,
                utilisateur.PhotoUrl))
            .ToListAsync(cancellationToken);

        return membres;
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
