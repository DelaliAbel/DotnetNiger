using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Une ressource, tel que la liste l'administration.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Description">Description courte, affichée dans les listes.</param>
/// <param name="Categorie">Catégorie : formation, tutoriel, guide, bibliothèque ou outil.</param>
/// <param name="Fournisseur">Éditeur ou source.</param>
/// <param name="LienExterne">Adresse de la ressource lorsqu'elle est partagée par un lien.</param>
/// <param name="EstInterne">Vrai si la ressource est rédigée sur le site.</param>
/// <param name="Statut">État de publication.</param>
/// <param name="Auteur">Nom du contributeur.</param>
/// <param name="PublieLe">Date de publication, ou <see langword="null"/> si jamais publiée.</param>
/// <param name="ModifieLe">Dernière modification, ou <see langword="null"/> si intacte.</param>
public sealed record LigneRessourceAdmin(
    Guid Id,
    string Slug,
    string Titre,
    string Description,
    ResourceCategory Categorie,
    string? Fournisseur,
    string? LienExterne,
    bool EstInterne,
    PublicationStatus Statut,
    string Auteur,
    DateTime? PublieLe,
    DateTime? ModifieLe);

/// <summary>Liste complète lue pour la page Ressources.</summary>
/// <param name="Ressources">Ressources, la plus récemment modifiée d'abord.</param>
public sealed record RessourcesVue(IReadOnlyList<LigneRessourceAdmin> Ressources);

/// <summary>Ressource complète, pour la reprise dans l'éditeur.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Categorie">Catégorie.</param>
/// <param name="Contenu">Contenu rédigé sur le site, ou <see langword="null"/>.</param>
/// <param name="LienExterne">Adresse de la ressource externe, ou <see langword="null"/>.</param>
/// <param name="Fournisseur">Éditeur ou source, ou <see langword="null"/>.</param>
/// <param name="ImageUrl">Image de couverture, ou <see langword="null"/>.</param>
public sealed record RessourceAdminDetail(
    Guid Id,
    string Titre,
    string Description,
    ResourceCategory Categorie,
    string? Contenu,
    string? LienExterne,
    string? Fournisseur,
    string? ImageUrl);

/// <summary>
/// Lecture des ressources pour l'administration. Les écritures passent par les points
/// d'extrémité de l'API, comme pour le blog et les événements.
/// </summary>
/// <remarks>
/// Contrairement à ces deux-là, la liste ne filtre aucun état : une ressource retirée du
/// site reste saisissable — une ressource archivée par erreur doit pouvoir revenir, et
/// l'administration est le seul endroit où elle est encore visible.
/// </remarks>
public sealed class ServiceRessources(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    private Task<RessourcesVue>? _lecture;

    /// <summary>Lit la liste des ressources, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les ressources de l'administration.</returns>
    public Task<RessourcesVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Lit une ressource en entier, pour l'ouvrir dans l'éditeur.</summary>
    /// <param name="ressourceId">Identifiant de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource, ou <see langword="null"/> si elle est introuvable.</returns>
    public async Task<RessourceAdminDetail?> LireDetailAsync(
        Guid ressourceId,
        CancellationToken cancellationToken = default)
    {
        var ressource = await uniteDeTravail.ReadRepository<Resource>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(candidat => candidat.Id == ressourceId, cancellationToken);

        return ressource is null
            ? null
            : new RessourceAdminDetail(
                ressource.Id,
                ressource.Title,
                ressource.Description,
                ressource.Category,
                ressource.Content,
                ressource.ExternalUrl,
                ressource.Provider,
                ressource.CoverImageUrl);
    }

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<RessourcesVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var ressources = await uniteDeTravail.ReadRepository<Resource>().Query
            .AsNoTracking()
            .OrderByDescending(ressource => ressource.UpdatedAtUtc ?? ressource.CreatedAtUtc)
            .ThenBy(ressource => ressource.Title)
            .ToListAsync(cancellationToken);

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. ressources.Select(candidat => candidat.CreatorId).Distinct()],
            cancellationToken);

        return new RessourcesVue(
            [.. ressources.Select(ressource => new LigneRessourceAdmin(
                ressource.Id,
                ressource.Slug,
                ressource.Title,
                ressource.Description,
                ressource.Category,
                ressource.Provider,
                ressource.ExternalUrl,
                string.IsNullOrWhiteSpace(ressource.Content) is false,
                ressource.Status,
                auteurs.GetValueOrDefault(ressource.CreatorId)?.FullName ?? string.Empty,
                ressource.PublishedAtUtc,
                ressource.UpdatedAtUtc))]);
    }
}
