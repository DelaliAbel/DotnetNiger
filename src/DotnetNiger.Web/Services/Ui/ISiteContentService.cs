namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Point d'accès aux contenus affichés par le site public.
/// </summary>
/// <remarks>
/// Phase 1 : <see cref="PlaceholderSiteContentService"/> renvoie des données de
/// garniture pour valider le rendu. Phase 2 : une implémentation branchée sur l'API
/// prendra le relais via le conteneur d'injection de dépendances, sans changer les pages.
/// </remarks>
public interface ISiteContentService
{
    Task<AccueilVue> ChargerAccueilAsync(CancellationToken annulation = default);

    Task<IReadOnlyList<ArticleVue>> ChargerArticlesAsync(CancellationToken annulation = default);

    Task<IReadOnlyList<EvenementVue>> ChargerEvenementsAsync(CancellationToken annulation = default);

    Task<IReadOnlyList<RessourceVue>> ChargerRessourcesAsync(CancellationToken annulation = default);

    Task<IReadOnlyList<ProjetVue>> ChargerProjetsAsync(CancellationToken annulation = default);

    Task<IReadOnlyList<PartenaireVue>> ChargerPartenairesAsync(CancellationToken annulation = default);

    Task<IReadOnlyList<MembreVue>> ChargerMembresAsync(CancellationToken annulation = default);

    Task<AProposVue> ChargerAProposAsync(CancellationToken annulation = default);
}