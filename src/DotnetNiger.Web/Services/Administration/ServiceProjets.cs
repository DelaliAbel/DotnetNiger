using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un projet, tel que la liste l'administration.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Nom">Nom du projet.</param>
/// <param name="Resume">Phrase d'accroche.</param>
/// <param name="Depot">Adresse du dépôt Git.</param>
/// <param name="Technologies">Technologies utilisées, séparées par des virgules.</param>
/// <param name="Statut">État de publication.</param>
/// <param name="Auteur">Nom du contributeur.</param>
/// <param name="PublieLe">Date de publication, ou <see langword="null"/> si jamais publié.</param>
/// <param name="ModifieLe">Dernière modification, ou <see langword="null"/> si intact.</param>
public sealed record LigneProjetAdmin(
    Guid Id,
    string Slug,
    string Nom,
    string Resume,
    string Depot,
    string? Technologies,
    PublicationStatus Statut,
    string Auteur,
    DateTime? PublieLe,
    DateTime? ModifieLe);

/// <summary>Liste complète lue pour la page Projets.</summary>
/// <param name="Projets">Projets, le plus récemment modifié d'abord.</param>
public sealed record ProjetsVue(IReadOnlyList<LigneProjetAdmin> Projets);

/// <summary>Projet complet, pour la reprise dans l'éditeur.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Nom">Nom du projet.</param>
/// <param name="Resume">Phrase d'accroche.</param>
/// <param name="Description">Description détaillée, au format HTML.</param>
/// <param name="Depot">Adresse du dépôt Git.</param>
/// <param name="Demonstration">Adresse de la démonstration en ligne, ou <see langword="null"/>.</param>
/// <param name="Technologies">Technologies utilisées, ou <see langword="null"/>.</param>
/// <param name="ImageUrl">Logo du projet, ou <see langword="null"/>.</param>
public sealed record ProjetAdminDetail(
    Guid Id,
    string Nom,
    string Resume,
    string Description,
    string Depot,
    string? Demonstration,
    string? Technologies,
    string? ImageUrl);

/// <summary>
/// Lecture des projets pour l'administration. Les écritures passent par les points
/// d'extrémité de l'API, comme pour les autres contenus.
/// </summary>
public sealed class ServiceProjets(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    private Task<ProjetsVue>? _lecture;

    /// <summary>Lit la liste des projets, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les projets de l'administration.</returns>
    public Task<ProjetsVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Lit un projet en entier, pour l'ouvrir dans l'éditeur.</summary>
    /// <param name="projetId">Identifiant du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet, ou <see langword="null"/> s'il est introuvable.</returns>
    public async Task<ProjetAdminDetail?> LireDetailAsync(
        Guid projetId,
        CancellationToken cancellationToken = default)
    {
        var projet = await uniteDeTravail.ReadRepository<Project>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(candidat => candidat.Id == projetId, cancellationToken);

        return projet is null
            ? null
            : new ProjetAdminDetail(
                projet.Id,
                projet.Name,
                projet.Summary,
                projet.Description,
                projet.RepositoryUrl,
                projet.DemoUrl,
                projet.Technologies,
                projet.ImageUrl);
    }

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<ProjetsVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var projets = await uniteDeTravail.ReadRepository<Project>().Query
            .AsNoTracking()
            .OrderByDescending(projet => projet.UpdatedAtUtc ?? projet.CreatedAtUtc)
            .ThenBy(projet => projet.Name)
            .ToListAsync(cancellationToken);

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. projets.Select(candidat => candidat.CreatedById).Distinct()],
            cancellationToken);

        return new ProjetsVue(
            [.. projets.Select(projet => new LigneProjetAdmin(
                projet.Id,
                projet.Slug,
                projet.Name,
                projet.Summary,
                projet.RepositoryUrl,
                projet.Technologies,
                projet.Status,
                auteurs.GetValueOrDefault(projet.CreatedById)?.FullName ?? string.Empty,
                projet.PublishedAtUtc,
                projet.UpdatedAtUtc))]);
    }
}
