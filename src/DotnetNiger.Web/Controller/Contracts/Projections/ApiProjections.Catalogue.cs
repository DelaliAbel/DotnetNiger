using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Queries;

namespace DotnetNiger.Web.Controller.Contracts;

internal static partial class ApiProjections
{
    /// <summary>
    /// Projette une ressource en résumé.
    /// </summary>
    /// <param name="resource">Ressource à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ResourceSummaryResponse ToSummary(Resource resource) =>
        new(
            resource.Id,
            resource.Slug,
            resource.Title,
            resource.Description,
            resource.Category,
            resource.Provider,
            resource.ExternalUrl,
            resource.CoverImageUrl);

    /// <summary>
    /// Projette une ressource en résumé d'administration, état de publication compris.
    /// </summary>
    /// <param name="resource">Ressource à projeter.</param>
    /// <param name="createur">Nom du créateur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static AdminResourceSummaryResponse ToAdminSummary(Resource resource, string createur) =>
        new(
            resource.Id,
            resource.Slug,
            resource.Title,
            resource.Description,
            resource.Category,
            resource.Provider,
            resource.ExternalUrl,
            resource.Status,
            createur,
            resource.PublishedAtUtc,
            resource.CoverImageUrl);

    /// <summary>
    /// Projette un projet en résumé.
    /// </summary>
    /// <param name="project">Projet à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ProjectSummaryResponse ToSummary(Project project) =>
        new(
            project.Id,
            project.Slug,
            project.Name,
            project.Summary,
            project.Technologies,
            project.RepositoryUrl,
            project.ImageUrl);

    /// <summary>
    /// Projette un projet en résumé d'administration, état de publication compris.
    /// </summary>
    /// <param name="project">Projet à projeter.</param>
    /// <param name="createur">Nom du créateur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static AdminProjectSummaryResponse ToAdminSummary(Project project, string createur) =>
        new(
            project.Id,
            project.Slug,
            project.Name,
            project.Summary,
            project.Technologies,
            project.RepositoryUrl,
            project.Status,
            createur,
            project.PublishedAtUtc,
            project.ImageUrl);

    /// <summary>
    /// Projette un partenaire.
    /// </summary>
    /// <param name="partner">Partenaire à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static PartnerResponse ToResponse(Partner partner) =>
        new(
            partner.Id,
            partner.Name,
            partner.Description,
            partner.WebsiteUrl,
            partner.LogoUrl,
            partner.Tier);

    /// <summary>
    /// Projette une ressource lue en base en résumé.
    /// </summary>
    /// <param name="ressource">Ressource à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ResourceSummaryResponse ToSummary(RessourceLu ressource) =>
        new(
            ressource.Id,
            ressource.Slug,
            ressource.Titre,
            ressource.Description,
            CategorieDepuisLibelle(ressource.Categorie),
            ressource.Provider,
            ressource.Lien,
            ressource.ImageUrl);

    /// <summary>
    /// Projette un projet lu en base en résumé.
    /// </summary>
    /// <param name="projet">Projet à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ProjectSummaryResponse ToSummary(ProjetLu projet) =>
        new(
            projet.Id,
            projet.Slug,
            projet.Nom,
            projet.Description,
            string.Join(", ", projet.Technologies),
            projet.Lien,
            projet.ImageUrl);

    /// <summary>
    /// Projette un partenaire lu en base.
    /// </summary>
    /// <param name="partenaire">Partenaire à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static PartnerResponse ToResponse(PartenaireLu partenaire) =>
        new(
            partenaire.Id,
            partenaire.Nom,
            partenaire.Description,
            partenaire.Lien ?? string.Empty,
            partenaire.LogoUrl,
            TierDepuisLibelle(partenaire.Type));

    /// <summary>
    /// Retrouve la catégorie d'une ressource à partir du libellé stocké en base.
    /// </summary>
    /// <param name="libelle">Libellé de la catégorie.</param>
    /// <returns>
    /// Catégorie correspondante, ou <see cref="ResourceCategory.Guide"/> si le libellé est
    /// inconnu. Une valeur par défaut évite qu'une catégorie ajoutée à l'énumération mais pas
    /// encore reconnue ici fasse échouer toute la liste des ressources.
    /// </returns>
    private static ResourceCategory CategorieDepuisLibelle(string libelle) =>
        libelle.ToLowerInvariant() switch
        {
            "training" or "cours" or "cours en ligne" => ResourceCategory.Training,
            "tutorial" or "tutoriel" => ResourceCategory.Tutorial,
            "library" or "bibliotheque" or "bibliothèque" => ResourceCategory.Library,
            "tool" or "outil" => ResourceCategory.Tool,
            _ => ResourceCategory.Guide,
        };

    /// <summary>
    /// Retrouve le statut du partenaire à partir du libellé stocké en base.
    /// </summary>
    /// <param name="libelle">Libellé du statut.</param>
    /// <returns>
    /// Statut correspondant, ou <see cref="PartnerTier.Partner"/> si le libellé est inconnu :
    /// un partenaire est le cas courant, et le libellé ne doit pas faire échouer la liste.
    /// </returns>
    private static PartnerTier TierDepuisLibelle(string libelle) =>
        libelle.ToLowerInvariant() switch
        {
            "sponsor" => PartnerTier.Sponsor,
            _ => PartnerTier.Partner,
        };
}
