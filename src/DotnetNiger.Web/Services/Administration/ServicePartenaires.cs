using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un partenaire, tel que la liste l'administration.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Nom">Nom de l'organisation.</param>
/// <param name="Description">Phrase de présentation.</param>
/// <param name="Site">Adresse du site officiel.</param>
/// <param name="Niveau">Sponsor ou partenaire.</param>
/// <param name="LogoUrl">Logo, ou <see langword="null"/>.</param>
/// <param name="Ordre">Ordre d'affichage.</param>
/// <param name="Actif">Vrai si le partenaire figure sur le site.</param>
public sealed record LignePartenaireAdmin(
    Guid Id,
    string Slug,
    string Nom,
    string Description,
    string Site,
    PartnerTier Niveau,
    string? LogoUrl,
    int Ordre,
    bool Actif);

/// <summary>Liste complète lue pour la page Partenaires.</summary>
/// <param name="Partenaires">Partenaires, sponsor d'abord puis par ordre d'affichage.</param>
public sealed record PartenairesVue(IReadOnlyList<LignePartenaireAdmin> Partenaires);

/// <summary>Partenaire complet, pour la reprise dans le formulaire.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Nom">Nom de l'organisation.</param>
/// <param name="Description">Phrase de présentation.</param>
/// <param name="Site">Adresse du site officiel.</param>
/// <param name="Niveau">Sponsor ou partenaire.</param>
/// <param name="LogoUrl">Logo, ou <see langword="null"/>.</param>
/// <param name="Ordre">Ordre d'affichage.</param>
public sealed record PartenaireAdminDetail(
    Guid Id,
    string Nom,
    string Description,
    string Site,
    PartnerTier Niveau,
    string? LogoUrl,
    int Ordre);

/// <summary>
/// Lecture des partenaires pour l'administration. Les écritures passent par les points
/// d'extrémité de l'API.
/// </summary>
/// <remarks>
/// Un partenaire n'a pas de cycle de publication : il est présent ou absent du site. La
/// liste montre donc les deux, car un partenaire masqué doit pouvoir revenir.
/// </remarks>
public sealed class ServicePartenaires(IUnitOfWork uniteDeTravail)
{
    private Task<PartenairesVue>? _lecture;

    /// <summary>Lit la liste des partenaires, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les partenaires de l'administration.</returns>
    public Task<PartenairesVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Lit un partenaire, pour l'ouvrir dans le formulaire.</summary>
    /// <param name="partenaireId">Identifiant du partenaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le partenaire, ou <see langword="null"/> s'il est introuvable.</returns>
    public async Task<PartenaireAdminDetail?> LireDetailAsync(
        Guid partenaireId,
        CancellationToken cancellationToken = default)
    {
        var partenaire = await uniteDeTravail.ReadRepository<Partner>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(candidat => candidat.Id == partenaireId, cancellationToken);

        return partenaire is null
            ? null
            : new PartenaireAdminDetail(
                partenaire.Id,
                partenaire.Name,
                partenaire.Description,
                partenaire.WebsiteUrl,
                partenaire.Tier,
                partenaire.LogoUrl,
                partenaire.SortOrder);
    }

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<PartenairesVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var partenaires = await uniteDeTravail.ReadRepository<Partner>().Query
            .AsNoTracking()
            // Les sponsors passent avant les partenaires, puis l'ordre choisi à la main
            // départage : c'est lui que l'administration règle, pas le nom.
            .OrderByDescending(partenaire => partenaire.Tier == PartnerTier.Sponsor)
            .ThenBy(partenaire => partenaire.SortOrder)
            .ThenBy(partenaire => partenaire.Name)
            .ToListAsync(cancellationToken);

        return new PartenairesVue(
            [.. partenaires.Select(partenaire => new LignePartenaireAdmin(
                partenaire.Id,
                partenaire.Slug,
                partenaire.Name,
                partenaire.Description,
                partenaire.WebsiteUrl,
                partenaire.Tier,
                partenaire.LogoUrl,
                partenaire.SortOrder,
                partenaire.IsActive))]);
    }
}
