using System.Diagnostics.CodeAnalysis;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Valeurs fournies à un modèle de courriel au moment du rendu.
/// </summary>
/// <remarks>
/// Un modèle ne lit ses valeurs que par cette classe, et n'en encode jamais rien.
/// <para>
/// L'encodage est l'affaire de la mise en page seule : elle échappe chaque paragraphe et
/// chaque adresse au moment de composer le HTML. Un modèle qui encoderait de son côté
/// produirait un texte doublement échappé — le membre verrait &amp;lt;b&amp;gt; dans sa
/// boîte au lieu de « &lt;b&gt; ». Une seule couche décide donc, et c'est la dernière.
/// </para>
/// <para>
/// Ce que les lectures garantissent malgré tout : une variable absente est signalée, et
/// une adresse de lien doit être absolue et en http ou https. Un nom de membre contenant
/// une balise ne casse donc pas le message, et une adresse truquée ne peut pas devenir un
/// lien vers un site pirate.
/// </para>
/// </remarks>
public sealed class EmailValues
{
    private readonly IReadOnlyDictionary<string, string> _valeurs;

    private EmailValues(IReadOnlyDictionary<string, string> valeurs) => _valeurs = valeurs;

    /// <summary>
    /// Ensemble vide, pour un modèle sans variable.
    /// </summary>
    public static EmailValues Vide { get; } = new(new Dictionary<string, string>(0));

    /// <summary>
    /// Compose un ensemble à partir de paires nom/valeur.
    /// </summary>
    /// <param name="paires">Paires fournies dans l'ordre.</param>
    /// <returns>L'ensemble de valeurs.</returns>
    public static EmailValues De(params (string Nom, string Valeur)[] paires)
    {
        ArgumentNullException.ThrowIfNull(paires);

        var valeurs = new Dictionary<string, string>(paires.Length, StringComparer.Ordinal);

        foreach (var (nom, valeur) in paires)
        {
            valeurs[nom] = valeur;
        }

        return new EmailValues(valeurs);
    }

    /// <summary>
    /// Compose un ensemble à partir d'un dictionnaire, en tolérant les clés inutilisées.
    /// </summary>
    /// <param name="valeurs">Valeurs à conserver.</param>
    /// <param name="nomsRetenus">Noms à ne pas conserver.</param>
    /// <returns>L'ensemble ne contenant que les noms retenus.</returns>
    public static EmailValues SousEnsemble(
        IReadOnlyDictionary<string, string> valeurs,
        IReadOnlyCollection<string> nomsRetenus)
    {
        ArgumentNullException.ThrowIfNull(valeurs);
        ArgumentNullException.ThrowIfNull(nomsRetenus);

        var retenues = new Dictionary<string, string>(nomsRetenus.Count, StringComparer.Ordinal);

        foreach (var nom in nomsRetenus)
        {
            if (valeurs.TryGetValue(nom, out var valeur))
            {
                retenues[nom] = valeur;
            }
        }

        return new EmailValues(retenues);
    }

    /// <summary>
    /// Indique si une valeur a été fournie.
    /// </summary>
    /// <param name="nom">Nom de la variable.</param>
    /// <returns>Vrai si la variable est présente.</returns>
    public bool Contient(string nom) => _valeurs.ContainsKey(nom);

    /// <summary>
    /// Rend une valeur destinée au corps d'un paragraphe, à un objet, ou à la version texte.
    /// </summary>
    /// <param name="nom">Nom de la variable.</param>
    /// <returns>Valeur telle qu'elle a été fournie.</returns>
    /// <exception cref="InvalidOperationException">La variable est absente.</exception>
    /// <remarks>
    /// La valeur est rendue brute : la mise en page l'échappe lorsqu'elle compose le HTML.
    /// </remarks>
    public string Texte(string nom) => Lire(nom);

    /// <summary>
    /// Rend une valeur destinée à un lien.
    /// </summary>
    /// <param name="nom">Nom de la variable.</param>
    /// <returns>Adresse absolue, sous sa forme d'origine.</returns>
    /// <exception cref="InvalidOperationException">
    /// La variable est absente, ou l'adresse n'est pas une URL absolue http ou https.
    /// </exception>
    /// <remarks>
    /// Un lien de courriel est forcément absolu : le client de messagerie ne connaît pas le
    /// site. Les schémas autres que http et https sont refusés, de sorte qu'une valeur
    /// forgée ne puisse pas produire un lien cliquable vers un script.
    /// </remarks>
    public string Lien(string nom)
    {
        var url = Lire(nom);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var adresse)
            || (adresse.Scheme != Uri.UriSchemeHttp && adresse.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"Le lien « {nom} » n'est pas une adresse absolue http ou https : « {url} ».");
        }

        return adresse.AbsoluteUri;
    }

    /// <summary>
    /// Rend une valeur destinée à un lien, sans échouer si elle est absente.
    /// </summary>
    /// <param name="nom">Nom de la variable.</param>
    /// <param name="url">Adresse absolue rendue, si la variable est présente et valide.</param>
    /// <returns>Vrai si un lien a pu être rendu.</returns>
    public bool EssayerLien(string nom, [NotNullWhen(true)] out string? url)
    {
        url = null;

        if (!Contient(nom))
        {
            return false;
        }

        try
        {
            url = Lien(nom);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private string Lire(string nom)
    {
        if (!_valeurs.TryGetValue(nom, out var valeur))
        {
            throw new InvalidOperationException(
                $"La variable « {nom} » est absente du modèle de courriel.");
        }

        return valeur;
    }
}
