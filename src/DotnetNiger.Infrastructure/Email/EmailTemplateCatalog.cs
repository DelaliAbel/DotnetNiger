using System.Collections.Frozen;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Catalogue des modèles de courriel du site.
/// </summary>
/// <remarks>
/// <para>
/// Un seul endroit décrit les seize messages, avec leur objet, leur texte et un jeu de
/// valeurs d'exemple. L'administration s'en sert pour relire et essayer chaque modèle sans
/// provoquer d'envoi réel, et le code qui expédie passe par
/// <see cref="EmailTemplate.Rendre"/> plutôt que par une chaîne HTML collée dans le service
/// concerné : un message ne peut donc pas partir avec un rendu différent de celui qui a été
/// prévisualisé.
/// </para>
/// <para>
/// Les modèles vivent en C# et non en base : leur contenu fait partie du produit, il est
/// relu comme le code, et il n'a pas à être modifiable par un administrateur sans contrôle.
/// </para>
/// <para>
/// Chaque famille de messages est décrite dans un fichier voisin, qui expose une fabrique.
/// Le catalogue, lui, est monté une seule fois dans un <see cref="Lazy{T}"/> : l'ordre dans
/// lequel le compilateur visits les parties d'une classe partielle ne garantit pas l'ordre
/// d'initialisation des champs, et une fabrication par méthode rend cette question sans
/// objet.
/// </para>
/// </remarks>
public static partial class EmailTemplateCatalog
{
    private static readonly Lazy<IReadOnlyList<EmailTemplate>> Catalogue = new(() =>
    [
        .. ModelesDuCompte(),
        .. ModelesDeLaLettre(),
        .. ModelesDesEvenements(),
        .. ModelesDeContenu(),
        .. ModelesDeContact(),
        ModeleReponseContact(),
    ]);

    /// <summary>
    /// Tous les modèles, dans l'ordre où l'administration les découvre.
    /// </summary>
    public static IReadOnlyList<EmailTemplate> Tous => Catalogue.Value;

    /// <summary>
    /// Index des modèles par clé.
    /// </summary>
    private static FrozenDictionary<string, EmailTemplate> Index { get; } =
        FrozenDictionary.ToFrozenDictionary(
            Tous.ToDictionary(modele => modele.Cle, StringComparer.Ordinal));

    /// <summary>
    /// Toutes les clés déclarées, à seule fin de test.
    /// </summary>
    public static IReadOnlyCollection<string> Cles { get; } = Index.Keys;

    /// <summary>
    /// Récupère un modèle par sa clé.
    /// </summary>
    /// <param name="cle">Clé du modèle.</param>
    /// <param name="modele">Le modèle trouvé.</param>
    /// <returns>Vrai si le modèle existe.</returns>
    public static bool Essayer(string cle, out EmailTemplate? modele)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cle);

        return Index.TryGetValue(cle, out modele);
    }

    /// <summary>
    /// Récupère un modèle par sa clé, ou échoue.
    /// </summary>
    /// <param name="cle">Clé du modèle.</param>
    /// <returns>Le modèle.</returns>
    /// <exception cref="InvalidOperationException">La clé ne correspond à aucun modèle.</exception>
    public static EmailTemplate Obtenir(string cle) =>
        Essayer(cle, out var modele) && modele is not null
            ? modele
            : throw new InvalidOperationException(
                $"Aucun modèle de courriel pour la clé « {cle} ».");
}