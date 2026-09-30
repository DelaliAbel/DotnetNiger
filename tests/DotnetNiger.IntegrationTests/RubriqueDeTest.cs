using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Fournit la rubrique dont un article a besoin.
/// </summary>
/// <remarks>
/// Un article référence une rubrique, et non une catégorie d'un enum figé. Les tests
/// doivent donc disposer d'une rubrique valide avant d'appeler l'API, qui refuse un
/// identifiant inconnu.
/// <para>
/// L'aide travaille sur le contexte qui va posséder l'article, et jamais sur un autre.
/// C'est la seule façon d'éviter deux pannes distinctes : passer une instance d'un
/// contexte à un autre fait réinsérer la ligne — et échouer sur l'unicité du slug —
/// et fait dire à EF qu'une entité de même identifiant est déjà suivie.
/// </para>
/// </remarks>
internal static class RubriqueDeTest
{
    /// <summary>
    /// Rubrique du contexte, créée à la première demande.
    /// </summary>
    /// <param name="context">Contexte qui va posséder l'article.</param>
    /// <returns>Rubrique suivie par ce contexte.</returns>
    public static Rubrique Pour(DotnetNigerDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Toujours une lecture fraîche : l'instance déjà suivie peut porter un état
        // « Added » d'un essai de sauvegarde avorté, et finirait réinsérée.
        var enBase = context.Rubriques.FirstOrDefault();
        if (enBase is not null)
        {
            return enBase;
        }

        var rubrique = new Rubrique("Nouveautés de .NET", "nouveautes");
        context.Rubriques.Add(rubrique);
        context.SaveChanges();
        return rubrique;
    }

    /// <summary>
    /// Identifiant de la rubrique du contexte.
    /// </summary>
    /// <param name="context">Contexte qui va posséder l'article.</param>
    /// <returns>Identifiant de la rubrique.</returns>
    public static Guid Id(DotnetNigerDbContext context) => Pour(context).Id;
}
