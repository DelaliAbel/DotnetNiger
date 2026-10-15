using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Identity;

/// <summary>
/// Photo de profil d'un membre, conservée en base.
/// </summary>
/// <remarks>
/// Le fichier vit en base et non dans <c>wwwroot</c>, servi par un point de terminaison
/// qui vérifie le propriétaire. Deux raisons : le site est déployé sur un hébergeur dont
/// le système de fichiers ne survit pas à un redéploiement, et un fichier lisible
/// directement par son URL deviendrait, par construction, la seule photo du site sans
/// contrôle d'accès.
/// <para>
/// La photo importée d'un fournisseur externe n'est pas copiée ici : elle reste servie à
/// son adresse d'origine, déjà publique. Ce type ne concerne que les fichiers que le
/// membre dépose lui-même.
/// </para>
/// </remarks>
public sealed class ProfilePhoto : TimestampedEntity
{
    /// <summary>Taille maximale d'une photo déposée : 2 Mo.</summary>
    public const int MaxBytes = 2 * 1024 * 1024;

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
    private ProfilePhoto()
    {
    }

    /// <summary>
    /// Initialise une photo à partir des octets déposés.
    /// </summary>
    /// <param name="membreId">Identifiant du membre propriétaire.</param>
    /// <param name="contenu">Octets de l'image.</param>
    /// <param name="typeContenu">Type MIME, vérifié avant l'appel.</param>
    /// <exception cref="DomainException">
    /// Le fichier est vide, trop lourd, ou d'un type non accepté.
    /// </exception>
    public ProfilePhoto(Guid membreId, byte[] contenu, string typeContenu)
    {
        ArgumentNullException.ThrowIfNull(contenu);
        ArgumentNullException.ThrowIfNull(typeContenu);

        if (membreId == Guid.Empty)
        {
            throw new DomainException("membre_invalide", "Le membre propriétaire est requis.");
        }

        if (contenu.Length is 0 or > MaxBytes)
        {
            throw new DomainException(
                "taille_invalide",
                $"La photo doit peser entre 1 octet et {MaxBytes / 1024 / 1024} Mo.");
        }

        if (!ContentTypes.Contains(typeContenu, StringComparer.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "format_invalide",
                "Format non accepté : utilisez JPEG, PNG ou WebP.");
        }

        MembreId = membreId;
        _contenu = contenu;
        _typeContenu = typeContenu;
    }

    /// <summary>Identifiant du membre propriétaire.</summary>
    public Guid MembreId { get; private set; }

    /// <summary>Octets de l'image.</summary>
    public byte[] Contenu => _contenu;

    /// <summary>Type MIME de l'image, à renvoyer tel quel au navigateur.</summary>
    public string TypeContenu => _typeContenu;

    /// <summary>Nombre d'octets, pour l'en-tête <c>Content-Length</c>.</summary>
    public int Taille => _contenu.Length;
}