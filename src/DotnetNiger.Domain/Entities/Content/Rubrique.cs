using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
/// Rubrique éditoriale du blog.
/// </summary>
/// <remarks>
/// Les rubriques vivaient dans un enum fermé de trois valeurs, ce qui rendait
/// l'ajout d'une rubrique dépendant d'une modification de code et d'une migration.
/// Or le blog compte treize rubriques et la rédaction doit pouvoir en choisir une
/// dans un menu déroulant : elles sont donc des données, pas des constantes.
/// <para>
/// Le nom est modifiable sans casser les URL : le <see cref="Slug"/>, lui, ne change
/// plus une fois attribué, et c'est lui que portent les liens de filtre.
/// </para>
/// </remarks>
public sealed class Rubrique : Entity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Rubrique()
    {
    }

    /// <summary>
    /// Initialise une rubrique à partir de son libellé.
    /// </summary>
    /// <param name="nom">Libellé affiché dans les menus et sur les cartes.</param>
    /// <exception cref="DomainException">Libellé absent ou trop long.</exception>
    public Rubrique(string nom)
    {
        Nom = Required(nom);
        Slug = SlugFactory.Create(Nom);
    }

    /// <summary>
    /// Obtient le libellé de la rubrique.
    /// </summary>
    public string Nom { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le slug utilisé dans les URL de filtre, unique et immuable.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la couleur du badge, parmi celles du thème.
    /// </summary>
    public string Couleur { get; private set; } = "violet";

    /// <summary>
    /// Obtient l'ordre d'affichage dans les menus déroulants.
    /// </summary>
    public int Ordre { get; private set; }

    /// <summary>
    /// Renomme la rubrique sans toucher à son slug : les liens déjà diffusés
    /// continuaient de fonctionner.
    /// </summary>
    /// <param name="nom">Nouveau libellé.</param>
    /// <exception cref="DomainException">Libellé absent ou trop long.</exception>
    public void Renommer(string nom) => Nom = Required(nom);

    /// <summary>
    /// Change la couleur du badge et l'ordre d'affichage.
    /// </summary>
    /// <param name="couleur">Couleur du badge.</param>
    /// <param name="ordre">Ordre d'affichage.</param>
    public void Organiser(string couleur, int ordre)
    {
        Couleur = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(couleur, nameof(couleur)),
            FieldLimits.Label,
            nameof(couleur));

        Ordre = ordre;
    }

    private static string Required(string nom) =>
        DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(nom, nameof(nom)),
            FieldLimits.Title,
            nameof(nom));
}
