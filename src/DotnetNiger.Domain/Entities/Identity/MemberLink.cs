using DotnetNiger.Domain.Common;

namespace DotnetNiger.Domain.Entities.Identity;

/// <summary>
/// Lien externe présenté sur le profil d'un membre.
/// </summary>
/// <remarks>
/// Un membre peut exposer autant de liens que nécessaire — portfolio, blog, GitHub,
/// LinkedIn, X, ou tout autre site. Chaque lien a un libellé indépendant du fournisseur,
/// au lieu de limiter le profil à trois champs fixes.
/// </remarks>
public sealed class MemberLink : TimestampedEntity
{
    /// <summary>Nombre maximal de liens présentés sur un profil.</summary>
    public const int MaximumPerMember = 12;

    private MemberLink()
    {
    }

    /// <summary>Crée un lien appartenant au membre.</summary>
    /// <param name="memberId">Identifiant du membre propriétaire.</param>
    /// <param name="label">Libellé affiché.</param>
    /// <param name="url">Adresse HTTP ou HTTPS.</param>
    /// <param name="sortOrder">Ordre d'affichage, à partir de zéro.</param>
    public MemberLink(Guid memberId, string label, string url, int sortOrder)
    {
        if (memberId == Guid.Empty)
        {
            throw new Domain.Exceptions.DomainException("member_required", "Le membre propriétaire est requis.");
        }

        MemberId = memberId;
        Update(label, url, sortOrder);
    }

    /// <summary>Identifiant du membre propriétaire.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Libellé présenté au visiteur.</summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>Adresse vers laquelle le lien mène.</summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>Ordre d'affichage du lien dans le profil.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Remplace les données du lien.</summary>
    /// <param name="label">Nouveau libellé.</param>
    /// <param name="url">Nouvelle adresse.</param>
    /// <param name="sortOrder">Nouvel ordre d'affichage.</param>
    public void Update(string label, string url, int sortOrder)
    {
        Label = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(label, nameof(label)).Trim(),
            FieldLimits.Label,
            nameof(label));
        Url = DomainGuard.AgainstHttpUrl(url, nameof(url));
        SortOrder = DomainGuard.AgainstNegative(sortOrder, nameof(sortOrder));
    }
}
