using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Résumé d'un projet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record ProjectSummaryResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string? Technologies,
    string RepositoryUrl,
    string? ImageUrl);

/// <summary>Projet tel que l'administration le voit.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="Status">État de publication.</param>
/// <param name="Creator">Nom du membre qui a créé le projet.</param>
/// <param name="PublishedAtUtc">Date de publication, si publié.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record AdminProjectSummaryResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string? Technologies,
    string RepositoryUrl,
    PublicationStatus Status,
    string Creator,
    DateTime? PublishedAtUtc,
    string? ImageUrl);

/// <summary>Demande de création d'un projet.</summary>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="DemoUrl">Démonstration en ligne.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record CreateProjectRequest(
    [Required, StringLength(160)] string Name,
    [Required, StringLength(500)] string Summary,
    [Required] string Description,
    [Required, StringLength(2048)] string RepositoryUrl,
    [StringLength(2048)] string? DemoUrl = null,
    [StringLength(500)] string? Technologies = null,
    [StringLength(2048)] string? ImageUrl = null);

/// <summary>Demande de mise à jour d'un projet.</summary>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="DemoUrl">Démonstration en ligne.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="ImageUrl">Image de présentation, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateProjectRequest(
    [Required, StringLength(160)] string Name,
    [Required, StringLength(500)] string Summary,
    [Required] string Description,
    [Required, StringLength(2048)] string RepositoryUrl,
    [StringLength(2048)] string? DemoUrl = null,
    [StringLength(500)] string? Technologies = null,
    [StringLength(2048)] string? ImageUrl = null);

