using System.Collections.Concurrent;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Queries;
using DotnetNiger.Web.Services.Membres;

namespace DotnetNiger.Web.Services.EspaceMembre;

/// <summary>
/// Toutes les données de l'espace membre d'un membre, lues en une fois.
/// </summary>
/// <remarks>
/// La mise en page et la page seeked la même chose au même moment : la fiche pour le nom et
/// l'avatar, les compteurs pour le bandeau, les listes pour le contenu. Lues séparément,
/// elles partageraient le même contexte de base et EF refuserait deux opérations
/// simultanées — ce qui est arrivé deux fois sur ce site.
/// <para>
/// Une tâche par membre, mémoïsée : le premier appelant lit, les autres attendent le même
/// résultat. La portée est celle du service, donc une requête HTTP ou un circuit Blazor :
/// rien n'est partagé d'un visiteur à l'autre.
/// </para>
/// </remarks>
public sealed class ServiceEspaceMembre(
    ICurrentUserService utilisateurCourant,
    ServiceMembres membres,
    CatalogueQueries catalogue,
    ServiceInscriptionsMembre inscriptions)
{
    private readonly ConcurrentDictionary<Guid, Task<EspaceMembreVue?>> _lectures = new();

    /// <summary>
    /// Lit les données de l'espace membre du membre connecté.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les données, ou <see langword="null"/> si le compte n'existe pas.</returns>
    public Task<EspaceMembreVue?> LireAsync(CancellationToken cancellationToken = default)
    {
        if (utilisateurCourant.UserId is not { } identifiant)
        {
            return Task.FromResult<EspaceMembreVue?>(null);
        }

        return _lectures.GetOrAdd(identifiant, id => LitAsync(id, cancellationToken));
    }

    /// <summary>
    /// Oublie les données mises en cache, pour qu'une modification soit lue au prochain rendu.
    /// </summary>
    public void Oublie()
    {
        if (utilisateurCourant.UserId is { } identifiant)
        {
            _lectures.TryRemove(identifiant, out _);
        }
    }

    private async Task<EspaceMembreVue?> LitAsync(Guid id, CancellationToken cancellationToken)
    {
        var profil = await membres.LireAsync(id, cancellationToken);

        if (profil is null)
        {
            return null;
        }

        // Les deux lectures sont séquentielles et non parallèles : le contexte est unique,
        // il ne peut pas traiter deux requêtes en même temps.
        var ressources = await catalogue.ListerRessourcesDuMembreAsync(id, cancellationToken);
        var reservations = await inscriptions.ListerAsync(id, cancellationToken);

        return new EspaceMembreVue(profil, ressources, reservations.Inscriptions);
    }
}

/// <summary>
/// Données affichées dans l'espace membre d'un membre connecté.
/// </summary>
/// <param name="Profil">Fiche du membre : nom, ville, photo, technologies.</param>
/// <param name="Ressources">Ressources déposées, toutes catégories et tous états.</param>
/// <param name="Rencontres">Rencontres réservées, de la plus proche à la plus lointaine.</param>
public sealed record EspaceMembreVue(
    MembreVue Profil,
    IReadOnlyList<RessourceMembreLu> Ressources,
    IReadOnlyList<InscriptionVue> Rencontres)
{
    /// <summary>
    /// Libellé de la catégorie, sous la forme attendue par le formulaire de correction.
    /// </summary>
    /// <remarks>
    /// La liste garde le libellé lisible ; le formulaire a besoin de la valeur de
    /// l'énumération. La conversion se fait ici, une fois, plutôt que dans chaque vue.
    /// </remarks>
    public static ResourceCategory? CategorieDepuisLibelle(string libelle) =>
        Enum.TryParse<ResourceCategory>(libelle, ignoreCase: true, out var categorie)
            ? categorie
            : null;
}