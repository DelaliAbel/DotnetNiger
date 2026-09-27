namespace DotnetNiger.Domain.Common;

/// <summary>
/// Entité identifiable du domaine.
/// </summary>
/// <remarks>
/// Les dépôts et l'unité de travail sont contraints sur cette interface, et non sur la
/// classe de base <see cref="Entity"/>. C'est ce qui permet d'inclure dans le modèle de
/// persistance des entités qui héritent d'une base de framework, comme
/// <c>ApplicationUser</c> qui dérive d'<c>IdentityUser&lt;Guid&gt;</c>.
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Obtient l'identifiant de l'entité.
    /// </summary>
    Guid Id { get; }
}
