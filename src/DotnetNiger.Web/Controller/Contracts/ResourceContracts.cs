using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Résumé d'une ressource.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="ExternalUrl">URL externe, si la ressource est un lien.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record ResourceSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Description,
    ResourceCategory Category,
    string? Provider,
    string? ExternalUrl,
    string? CoverImageUrl);

/// <summary>Ressource telle que l'administration la voit.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="ExternalUrl">URL externe, si la ressource est un lien.</param>
/// <param name="Status">État de publication.</param>
/// <param name="Creator">Nom du membre qui a créé la ressource.</param>
/// <param name="PublishedAtUtc">Date de publication, si publiée.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record AdminResourceSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Description,
    ResourceCategory Category,
    string? Provider,
    string? ExternalUrl,
    PublicationStatus Status,
    string Creator,
    DateTime? PublishedAtUtc,
    string? CoverImageUrl);

/// <summary>Demande de création d'une ressource.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Content">Contenu rédigé, si la ressource est hébergée sur le site.</param>
/// <param name="ExternalUrl">URL de la ressource externe, si applicable.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record CreateResourceRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Description,
    ResourceCategory Category,
    // Le contenu est accepte des la creation : sans lui, une ressource redigee sur le
    // site ne pourrait etre enregistree qu'a la mise a jour, donc en deux enregistrements.
    [StringLength(32000)] string? Content = null,
    [StringLength(2048)] string? ExternalUrl = null,
    [StringLength(120)] string? Provider = null,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Demande de mise à jour d'une ressource.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Content">Contenu rédigé, si la ressource est hébergée sur le site.</param>
/// <param name="ExternalUrl">URL de la ressource externe.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="CoverImageUrl">Image de couverture, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateResourceRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Description,
    ResourceCategory Category,
    [StringLength(32000)] string? Content = null,
    [StringLength(2048)] string? ExternalUrl = null,
    [StringLength(120)] string? Provider = null,
    [StringLength(2048)] string? CoverImageUrl = null);

