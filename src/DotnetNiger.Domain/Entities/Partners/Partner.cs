using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Domain.Entities.Partners;

/// <summary>
/// Partenaire local ou sponsor de la communauté, affiché sur la page Partenaires et Sponsors.
/// </summary>
/// <remarks>
/// Géré exclusivement par les administrateurs. Un partenaire inactif reste en base mais
/// n'est pas affiché, ce qui permet de le retirer temporairement sans le ressaisir.
/// </remarks>
public sealed class Partner : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Partner()
    {
    }

    /// <summary>
    /// Initialise un nouveau partenaire actif.
    /// </summary>
    /// <param name="name">Nom de l'organisation.</param>
    /// <param name="description">Description de son soutien à la communauté.</param>
    /// <param name="websiteUrl">Site officiel.</param>
    /// <param name="tier">Niveau de partenariat.</param>
    public Partner(string name, string description, string websiteUrl, PartnerTier tier)
    {
        Name = Required(name, FieldLimits.Name, nameof(name));
        Description = Required(description, FieldLimits.Excerpt, nameof(description));
        WebsiteUrl = DomainGuard.AgainstHttpUrl(websiteUrl, nameof(websiteUrl));
        Tier = tier;
        Slug = SlugFactory.Create(name);
        IsActive = true;
    }

    /// <summary>
    /// Obtient le nom de l'organisation.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug d'URL, unique dans la base.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la description du soutien apporté.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le site officiel.
    /// </summary>
    public string WebsiteUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'URL du logo, stockée en base.
    /// </summary>
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Obtient le niveau de partenariat.
    /// </summary>
    public PartnerTier Tier { get; private set; }

    /// <summary>
    /// Obtient l'ordre d'affichage, du plus petit au plus grand.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Obtient une valeur indiquant si le partenaire est affiché sur le site.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Met à jour les informations du partenaire.
    /// </summary>
    /// <param name="name">Nouveau nom.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <param name="websiteUrl">Nouveau site officiel.</param>
    public void UpdateDetails(string name, string description, string websiteUrl)
    {
        var newName = Required(name, FieldLimits.Name, nameof(name));

        if (!string.Equals(Name, newName, StringComparison.Ordinal))
        {
            Name = newName;
            Slug = SlugFactory.Create(newName);
        }

        Description = Required(description, FieldLimits.Excerpt, nameof(description));
        WebsiteUrl = DomainGuard.AgainstHttpUrl(websiteUrl, nameof(websiteUrl));
    }

    /// <summary>
    /// Définit le logo du partenaire.
    /// </summary>
    /// <param name="logoUrl">URL absolue, ou <see langword="null"/> pour retirer le logo.</param>
    public void SetLogoUrl(string? logoUrl)
        => LogoUrl = DomainGuard.OptionalAgainstHttpUrl(logoUrl, nameof(logoUrl));

    /// <summary>
    /// Change le niveau de partenariat.
    /// </summary>
    /// <param name="tier">Nouveau niveau.</param>
    public void ChangeTier(PartnerTier tier) => Tier = tier;

    /// <summary>
    /// Définit l'ordre d'affichage.
    /// </summary>
    /// <param name="sortOrder">Position dans la liste.</param>
    public void SetSortOrder(int sortOrder)
        => SortOrder = DomainGuard.AgainstNegative(sortOrder, nameof(sortOrder));

    /// <summary>
    /// Affiche le partenaire sur le site.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Masque le partenaire sans le supprimer.
    /// </summary>
    public void Deactivate() => IsActive = false;

    private static string Required(string value, int maxLength, string field)
        => DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(value, field),
            maxLength,
            field);
}
