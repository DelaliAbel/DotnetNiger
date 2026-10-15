using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
/// Image déposée par un administrateur pour le contenu du site : couverture d'un article,
/// illustration d'une actualité, image insérée dans un article de blog.
/// </summary>
/// <remarks>
/// Le fichier vit en base et non dans <c>wwwroot</c>, servi par un point de terminaison
/// dédié — même raisonnement que les photos de profil : l'hébergeur ne survit pas à un
/// redéploiement du système de fichiers, et un fichier lisible directement par son URL
/// échapperait à tout contrôle. Contrairement aux photos, l'image de contenu est publique
/// dès qu'elle est mise en ligne : une couverture d'article se voit par les visiteurs, et
/// le point de terminaison qui la sert est donc ouvert.
/// <para>
/// Aucune image n'est liée à sa publication : la couverture et le corps d'un article
/// référencent l'image par son adresse, et rien n'empêche de la retirer d'un contenu
/// pendant qu'un autre la référence encore. Le ménage des orphelins est laissé au
/// site, hors du flux de rédaction.
/// </para>
/// </remarks>
public sealed class ContentImage : TimestampedEntity
{
    /// <summary>Taille maximale d'une image déposée : 5 Mo.</summary>
    public const int MaxBytes = 5 * 1024 * 1024;

    /// <summary>Types d'image acceptés, déduits du contenu réel du fichier.</summary>
    /// <remarks>
    /// La liste est volontairement courte et sans SVG : un SVG est un document actif, et
    /// servi depuis une origine du site il pourrait exécuter du script.
    /// </remarks>
    public static IReadOnlyList<string> ContentTypes { get; } =
        ["image/jpeg", "image/png", "image/webp"];

    private byte[] _contenu = [];

    private string _typeContenu = string.Empty;

    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private ContentImage()
    {
    }

    /// <summary>
    /// Initialise une image à partir des octets déposés.
    /// </summary>
    /// <param name="contenu">Octets de l'image.</param>
    /// <param name="typeContenu">Type MIME réel, vérifié avant l'appel.</param>
    /// <exception cref="DomainException">
    /// Le fichier est vide, trop lourd, ou d'un type non accepté.
    /// </exception>
    public ContentImage(byte[] contenu, string typeContenu)
    {
        ArgumentNullException.ThrowIfNull(contenu);
        ArgumentNullException.ThrowIfNull(typeContenu);

        if (contenu.Length is 0 or > MaxBytes)
        {
            throw new DomainException(
                "taille_invalide",
                $"L'image doit peser entre 1 octet et {MaxBytes / 1024 / 1024} Mo.");
        }

        if (!ContentTypes.Contains(typeContenu, StringComparer.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "format_invalide",
                "Format non accepté : utilisez JPEG, PNG ou WebP.");
        }

        _contenu = contenu;
        _typeContenu = typeContenu;
    }

    /// <summary>Octets de l'image.</summary>
    public byte[] Contenu => _contenu;

    /// <summary>Type MIME de l'image, à renvoyer tel quel au navigateur.</summary>
    public string TypeContenu => _typeContenu;

    /// <summary>Nombre d'octets, pour l'en-tête <c>Content-Length</c>.</summary>
    public int Taille => _contenu.Length;
}