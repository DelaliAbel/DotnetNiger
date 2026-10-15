using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Identity;
using DotnetNiger.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Content;

/// <summary>
/// Dépôt et lecture des photos de profil.
/// </summary>
/// <remarks>
/// Le fichier est remplacé, pas versionné : un membre n'a qu'une photo active, et garder
/// les anciennes ne lui apporte rien puisque rien ne les référence.
/// </remarks>
public sealed class DepotPhotosProfil(IUnitOfWork uniteDeTravail)
{
    /// <summary>
    /// Remplace la photo d'un membre par celle qu'il vient de déposer.
    /// </summary>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <param name="contenu">Octets de l'image.</param>
    /// <param name="typeContenu">Type MIME annoncé par le fichier.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La photo désormais active.</returns>
    /// <remarks>
    /// Un fichier dont le type n'est pas reconnu est refusé, et non enregistré sous le type
    /// annoncé par le navigateur. Retomber sur la déclaration du client annulerait la
    /// vérification : n'importe quel fichier peut être envoyé en se déclarant
    /// <c>image/jpeg</c>, ce qui est précisément ce qu'on cherche à empêcher.
    /// </remarks>
    /// <exception cref="DomainException">Le contenu n'est pas une image reconnue.</exception>
    public async Task<ProfilePhoto> RemplacerAsync(
        Guid membreId,
        byte[] contenu,
        string typeContenu,
        CancellationToken cancellationToken = default)
    {
        if (FormatsImage.LireTypeReel(contenu) is not { } type)
        {
            throw new DomainException(
                "format_invalide",
                "Le fichier n'est pas une image JPEG, PNG ou WebP.");
        }

        var photo = new ProfilePhoto(membreId, contenu, type);

        var precedente = await uniteDeTravail.ReadRepository<ProfilePhoto>().Query
            .FirstOrDefaultAsync(actuelle => actuelle.MembreId == membreId, cancellationToken);

        if (precedente is not null)
        {
            uniteDeTravail.Repository<ProfilePhoto>().Remove(precedente);
        }

        uniteDeTravail.Repository<ProfilePhoto>().Add(photo);
        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return photo;
    }

    /// <summary>
    /// Retire la photo d'un membre, qui retombe alors sur ses initiales.
    /// </summary>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si une photo existait.</returns>
    public async Task<bool> SupprimerAsync(Guid membreId, CancellationToken cancellationToken = default)
    {
        var photo = await uniteDeTravail.ReadRepository<ProfilePhoto>().Query
            .FirstOrDefaultAsync(actuelle => actuelle.MembreId == membreId, cancellationToken);

        if (photo is null)
        {
            return false;
        }

        uniteDeTravail.Repository<ProfilePhoto>().Remove(photo);
        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Lit la photo d'un membre, pour l'afficher.
    /// </summary>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La photo, ou <see langword="null"/> si le membre n'en a pas.</returns>
    public async Task<ProfilePhoto?> LireAsync(Guid membreId, CancellationToken cancellationToken = default) =>
        await uniteDeTravail.ReadRepository<ProfilePhoto>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(photo => photo.MembreId == membreId, cancellationToken);
}