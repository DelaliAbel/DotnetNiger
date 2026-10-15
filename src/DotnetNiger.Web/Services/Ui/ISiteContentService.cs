namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Point d'accès aux contenus affichés par le site public.
/// </summary>
/// <remarks>
/// Une seule source de vérité : la base, via <c>ContenuSiteService</c>. Les contenus
/// publiés au back-office apparaissent donc immédiatement, et une page ne peut pas afficher
/// autre chose que ce que l'API renvoie.
/// <para>
/// Le contenu se saisit depuis l'administration. Une base fraîche démarre donc vide, et les
/// pages doivent savoir dire « rien de published pour l'instant » plutôt que proposer une
/// garniture : un article factice en production est pire qu'une page vide, parce qu'elle
/// passe pour un vrai contenu.
/// </para>
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

    Task<CommunauteVue> ChargerCommunauteAsync(CancellationToken annulation = default);
}