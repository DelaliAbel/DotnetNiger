using DotnetNiger.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Alimente la table des rubriques du blog au premier démarrage.
/// </summary>
/// <remarks>
/// Les rubriques sont des données et non des constantes : le menu déroulant de la
/// rédaction s'y alimente, et l'ajout d'une rubrique ne demande ni migration ni
/// redéploiement. Encore faut-il que la table soit peuplée sur une base neuve, d'où ce
/// amorçage.
/// <para>
/// L'opération est idempotente et additive : seules les rubriques absentes sont
/// insérées, et jamais dans l'ordre d'un libellé mais selon la liste de référence. Une
/// rubrique renommée ou déplacée par un administrateur n'est donc pas écrasée au
/// redémarrage — sinon ses choix seraient perdus à chaque déploiement.
/// </para>
/// </remarks>
public sealed partial class RubriqueBootstrapper(
    IServiceProvider services,
    ILogger<RubriqueBootstrapper> logger)
{
    /// <summary>
    /// Rubriques de référence, dans l'ordre d'affichage.
    /// </summary>
    /// <remarks>
    /// Le slug est explicite plutôt que dérivé : ces URL sont déjà partagées dans des
    /// conversations et des liens, les changer casserait ces renvois sans raison.
    /// </remarks>
    private static readonly (string Slug, string Nom, string Couleur)[] RubriquesDeReference =
    [
        ("nouveautes", "Nouveautés de .NET", "violet"),
        ("csharp", "C#", "violet"),
        ("blazor", "Blazor", "violet"),
        ("aspnet-core", "ASP.NET Core", "violet"),
        ("architecture", "Architecture & performance", "orange"),
        ("securite", "Sécurité", "orange"),
        ("devops", "DevOps & déploiement", "ink"),
        ("donnees", "Données & Entity Framework", "vert"),
        ("tests", "Tests & qualité", "vert"),
        ("outils", "Outils & bibliothèques", "vert"),
        ("interviews", "Interviews", "orange"),
        ("projets", "Projets locaux", "vert"),
        ("retours", "Retours d'expérience", "ink"),
    ];

    /// <summary>
    /// Insère les rubriques de référence manquantes.
    /// </summary>
    /// <returns>Tâche complète une fois la base à jour.</returns>
    public async Task InitializeAsync()
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();

        var existantes = await context.Rubriques
            .Select(r => r.Slug)
            .ToListAsync();

        var manquantes = RubriquesDeReference
            .Where(r => !existantes.Contains(r.Slug))
            .ToArray();

        if (manquantes.Length == 0)
        {
            return;
        }

        // L'ordre suit la liste de référence, pas l'ordre d'insertion : deux
        // démarrages successifs doivent produire le même résultat.
        var ordreDepart = await context.Rubriques.AnyAsync()
            ? await context.Rubriques.MaxAsync(r => r.Ordre) + 1
            : 0;

        for (var index = 0; index < manquantes.Length; index++)
        {
            var (slug, nom, couleur) = manquantes[index];
            var rubrique = new Rubrique(nom, slug);
            rubrique.Organiser(couleur, ordreDepart + index);
            context.Rubriques.Add(rubrique);
        }

        await context.SaveChangesAsync();

        RubriquesAmorcees(logger, manquantes.Length);
    }

    // Le nombre figure dans le message : il varie d'un démarrage à l'autre, et c'est
    // la seule information qui distingue un amorçage d'une base déjà peuplée.
    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Information,
        Message = "Rubriques du blog amorcées : {Nombre}")]
    private static partial void RubriquesAmorcees(ILogger logger, int nombre);
}
