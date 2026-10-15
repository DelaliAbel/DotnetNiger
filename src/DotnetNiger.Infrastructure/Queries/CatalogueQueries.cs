using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Queries;

/// <summary>Ressource publiée, telle que la sert l'API et la lit le catalogue.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Categorie">Catégorie (formation, tutoriel, guide, bibliothèque, outil).</param>
/// <param name="Sujet">Étiquette technique.</param>
/// <param name="Provider">Éditeur ou source de la ressource.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Lien">Adresse de la ressource.</param>
/// <param name="Externe">Vrai si la ressource quitte le site.</param>
/// <param name="ImageUrl">Vignette.</param>
/// <param name="Auteur">Nom du créateur.</param>
public sealed record RessourceLu(
    Guid Id,
    string Slug,
    string Titre,
    string Categorie,
    string Sujet,
    string? Provider,
    string Description,
    string? Lien,
    bool Externe,
    string? ImageUrl,
    string Auteur);

/// <summary>Ressource déposée par un membre, pour son tableau de bord.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Categorie">Libellé de la catégorie.</param>
/// <param name="Lien">Adresse externe, si la ressource vit ailleurs.</param>
/// <param name="ImageUrl">Illustration, si elle existe.</param>
/// <param name="Statut">État de publication.</param>
/// <param name="PublieeUtc">Date de publication, si publiée.</param>
public sealed record RessourceMembreLu(
    Guid Id,
    string Slug,
    string Titre,
    string Description,
    string Categorie,
    string? Lien,
    string? ImageUrl,
    PublicationStatus Statut,
    DateTime? PublieeUtc);

/// <summary>Projet publié, tel que la sert l'API et le lit la page communauté.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Nom">Nom du projet.</param>
/// <param name="Description">Résumé.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="Lien">Dépôt.</param>
/// <param name="ImageUrl">Vignette.</param>
public sealed record ProjetLu(
    Guid Id,
    string Slug,
    string Nom,
    string Description,
    IReadOnlyList<string> Technologies,
    string Lien,
    string? ImageUrl);

/// <summary>Partenaire ou sponsor, tel que la sert l'API et le lit la page dédiée.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Nom">Nom de l'organisation.</param>
/// <param name="Type">« Partenaire » ou « Sponsor ».</param>
/// <param name="Description">Présentation.</param>
/// <param name="LogoUrl">Logo.</param>
/// <param name="Lien">Site web.</param>
public sealed record PartenaireLu(
    Guid Id,
    string Slug,
    string Nom,
    string Type,
    string Description,
    string? LogoUrl,
    string? Lien);

/// <summary>
/// Lecture des ressources, des projets et des partenaires.
/// </summary>
/// <remarks>
/// Les trois catalogues suivent la même règle que le blog et l'agenda : l'API et les
/// pages publiques lisent la même source, pour qu'une ressource retirée de la base
/// disparaisse partout au même endroit.
/// </remarks>
public sealed class CatalogueQueries(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    /// <summary>Ressources publiées, filtrées et triées.</summary>
    /// <param name="categorie">Ne garder que cette catégorie.</param>
    /// <param name="recherche">Texte cherché dans le titre, la description et l'éditeur.</param>
    /// <param name="trier">Ordre demandé ; par défaut, le plus récent d'abord.</param>
    /// <param name="page">Page demandée, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les ressources de la page demandée, et le nombre total correspondant.</returns>
    public async Task<(IReadOnlyList<RessourceLu> Ressources, int Total)> ListerRessourcesAsync(
        ResourceCategory? categorie,
        string? recherche,
        string? trier,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var requete = uniteDeTravail.ReadRepository<Resource>().Query
            .Where(resource => resource.Status == PublicationStatus.Published);

        if (categorie is { } valeur)
        {
            requete = requete.Where(resource => resource.Category == valeur);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(resource =>
                resource.Title.Contains(terme)
                || resource.Description.Contains(terme)
                || (resource.Provider != null && resource.Provider.Contains(terme)));
        }

        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(resource => resource.PublishedAtUtc)
                .ThenBy(resource => resource.Title),
            "titre" => requete.OrderBy(resource => resource.Title),
            "titre-desc" => requete.OrderByDescending(resource => resource.Title),
            _ => requete
                .OrderByDescending(resource => resource.PublishedAtUtc)
                .ThenBy(resource => resource.Title),
        };

        var total = await requete.CountAsync(cancellationToken);

        var ressources = await requete
            .Skip((Math.Max(page, 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. ressources.Select(resource => resource.CreatorId).Distinct()],
            cancellationToken);

        return ([.. ressources.Select(resource => new RessourceLu(
            resource.Id,
            resource.Slug,
            resource.Title,
            LibelleCategorie(resource.Category),
            resource.Subject ?? string.Empty,
            resource.Provider,
            resource.Description,
            resource.ExternalUrl,
            !string.IsNullOrWhiteSpace(resource.ExternalUrl),
            resource.CoverImageUrl,
            auteurs.GetValueOrDefault(resource.CreatorId)?.FullName ?? string.Empty))], total);
    }

    /// <summary>Ressources partagées par un membre, toutes catégories et tous états.</summary>
    /// <remarks>
    /// La page membre montre ce que le membre a déposé, y compris ce qu'un administrateur a
    /// remis en brouillon : sans cela, une ressource retirée disparaîtrait de l'écran du
    /// membre sans qu'il puisse comprendre pourquoi.
    /// </remarks>
    /// <param name="auteurId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les ressources du membre, les plus récentes d'abord.</returns>
    public async Task<IReadOnlyList<RessourceMembreLu>> ListerRessourcesDuMembreAsync(
        Guid auteurId,
        CancellationToken cancellationToken = default)
    {
        var ressources = await uniteDeTravail.ReadRepository<Resource>().Query
            .Where(ressource => ressource.CreatorId == auteurId)
            // Un brouillon n'a pas de date de publication : il passe après les ressources
            // publiées plutôt que de remonter en tête avec une date fictive.
            .OrderByDescending(ressource => ressource.PublishedAtUtc ?? DateTime.MinValue)
            .ToListAsync(cancellationToken);

        return [.. ressources.Select(resource => new RessourceMembreLu(
            resource.Id,
            resource.Slug,
            resource.Title,
            resource.Description,
            LibelleCategorie(resource.Category),
            resource.ExternalUrl,
            resource.CoverImageUrl,
            resource.Status,
            resource.PublishedAtUtc))];
    }

    /// <summary>Projets publiés, filtrés et triés.</summary>
    /// <param name="recherche">Texte cherché dans le nom et le résumé.</param>
    /// <param name="technologies">Ne garder que les projets utilisant toutes ces technologies.</param>
    /// <param name="trier">Ordre demandé ; par défaut, le plus récent d'abord.</param>
    /// <param name="page">Page demandée, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les projets de la page demandée, et le nombre total correspondant.</returns>
    public async Task<(IReadOnlyList<ProjetLu> Projets, int Total)> ListerProjetsAsync(
        string? recherche,
        string? technologies,
        string? trier,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var requete = uniteDeTravail.ReadRepository<Project>().Query
            .Where(project => project.Status == PublicationStatus.Published);

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(project =>
                project.Name.Contains(terme) || project.Summary.Contains(terme));
        }

        if (!string.IsNullOrWhiteSpace(technologies))
        {
            foreach (var technologie in technologies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                requete = requete.Where(project => project.Technologies != null && project.Technologies.Contains(technologie));
            }
        }

        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(project => project.PublishedAtUtc)
                .ThenBy(project => project.Name),
            "titre" => requete.OrderBy(project => project.Name),
            "titre-desc" => requete.OrderByDescending(project => project.Name),
            _ => requete
                .OrderByDescending(project => project.PublishedAtUtc)
                .ThenBy(project => project.Name),
        };

        var total = await requete.CountAsync(cancellationToken);

        var projets = await requete
            .Skip((Math.Max(page, 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ([.. projets.Select(project => new ProjetLu(
            project.Id,
            project.Slug,
            project.Name,
            project.Summary,
            [.. (project.Technologies ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
            project.RepositoryUrl,
            project.ImageUrl))], total);
    }

    /// <summary>Partenaires et sponsors actifs, par ordre d'affichage.</summary>
    /// <param name="tier">Ne garder que les partenaires, ou que les sponsors.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les organisations correspondantes.</returns>
    public async Task<IReadOnlyList<PartenaireLu>> ListerPartenairesAsync(
        PartnerTier? tier,
        CancellationToken cancellationToken = default)
    {
        var requete = uniteDeTravail.ReadRepository<Partner>().Query
            .Where(partner => partner.IsActive);

        if (tier is { } niveau)
        {
            requete = requete.Where(partner => partner.Tier == niveau);
        }

        var partenaires = await requete
            .OrderBy(partner => partner.SortOrder)
            .ThenBy(partner => partner.Name)
            .ToListAsync(cancellationToken);

        return [.. partenaires.Select(partner => new PartenaireLu(
            partner.Id,
            partner.Slug,
            partner.Name,
            partner.Tier == PartnerTier.Sponsor ? "Sponsor" : "Partenaire",
            partner.Description,
            partner.LogoUrl,
            partner.WebsiteUrl))];
    }

    /// <summary>
    /// Libellé lisible d'une catégorie de ressource. L'énumération est en anglais, comme
    /// le reste du code ; l'interface parle français, et le lecteur aussi.
    /// </summary>
    private static string LibelleCategorie(ResourceCategory categorie) => categorie switch
    {
        ResourceCategory.Training => "Formation",
        ResourceCategory.Tutorial => "Tutoriel",
        ResourceCategory.Guide => "Guide",
        ResourceCategory.Library => "Bibliothèque",
        ResourceCategory.Tool => "Outil",
        _ => "Ressource",
    };
}
