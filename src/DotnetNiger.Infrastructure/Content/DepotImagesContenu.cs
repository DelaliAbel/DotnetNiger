using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Content;

/// <summary>
/// Dépôt et lecture des images de contenu déposées par les administrateurs.
/// </summary>
/// <remarks>
/// Une image est conservée telle qu'elle a été déposée : les contenus la référencent par son
/// adresse, et rien ici ne décide de sa durée de vie. Une image retirée d'un article n'est
/// pas effacée pour autant — un autre contenu peut encore la montrer, et une suppression
/// automatique risquerait de casser une page en ligne.
/// </remarks>
public sealed class DepotImagesContenu(IUnitOfWork uniteDeTravail)
{
    /// <summary>
    /// Dépose une image de contenu.
    /// </summary>
    /// <param name="contenu">Octets de l'image.</param>
    /// <param name="typeContenu">Type MIME annoncé par le fichier.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'image déposée.</returns>
    /// <remarks>
    /// Un fichier dont le type n'est pas reconnu est refusé, et non enregistré sous le type
    /// annoncé par le navigateur. Retomber sur la déclaration du client annulerait la
    /// vérification : n'importe quel fichier peut être envoyé en se déclarant
    /// <c>image/jpeg</c>, ce qui est précisément ce qu'on cherche à empêcher.
    /// </remarks>
    /// <exception cref="DomainException">Le contenu n'est pas une image reconnue.</exception>
    public async Task<ContentImage> DeposerAsync(
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

        var image = new ContentImage(contenu, type);

        uniteDeTravail.Repository<ContentImage>().Add(image);
        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return image;
    }

    /// <summary>
    /// Lit une image de contenu, pour l'afficher.
    /// </summary>
    /// <param name="id">Identifiant de l'image.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'image, ou <see langword="null"/> si elle n'existe pas.</returns>
    public async Task<ContentImage?> LireAsync(Guid id, CancellationToken cancellationToken = default) =>
        await uniteDeTravail.ReadRepository<ContentImage>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
}