using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
/// Un réglage du site : le logo, l'adresse de contact, les canaux de la communauté.
/// </summary>
/// <remarks>
/// Le site lit ses réglages en base plutôt que dans un fichier, parce que ce sont des
/// valeurs qu'une administratrice doit pouvoir changer sans toucher au serveur. Le fichier
/// garde ce qu'elle ne doit pas changer depuis le site : la base de données, le serveur de
/// courriel, les clés de connexion externe.
/// </remarks>
public sealed class ReglageSite : Entity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private ReglageSite()
    {
    }

    /// <summary>
    /// Crée un réglage, ou remplace la valeur d'un réglage existant.
    /// </summary>
    /// <param name="cle">Clé du réglage, parmi celles de <see cref="ClesReglage"/>.</param>
    /// <param name="valeur">Valeur, ou <see langword="null"/> pour retirer le réglage.</param>
    /// <exception cref="DomainException">
    /// La clé est inconnue, la valeur dépasse la longueur admise, ou la valeur ressemble à
    /// une adresse que le navigateur exécuterait.
    /// </exception>
    public ReglageSite(string cle, string? valeur)
    {
        Cle = VerifierLaCle(cle);
        Valeur = VerifierLaValeur(valeur);
    }

    /// <summary>
    /// Obtient la clé du réglage.
    /// </summary>
    public string Cle { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la valeur du réglage, ou <see langword="null"/> s'il n'est pas renseigné.
    /// </summary>
    public string? Valeur { get; private set; }

    /// <summary>
    /// Remplace la valeur du réglage. Une valeur vide équivaut à une absence : le réglage
    /// disparaît alors du site.
    /// </summary>
    /// <param name="valeur">Nouvelle valeur.</param>
    public void Definir(string? valeur) => Valeur = VerifierLaValeur(valeur);

    private static string VerifierLaCle(string cle)
    {
        if (string.IsNullOrWhiteSpace(cle))
        {
            throw new DomainException("setting_key_required", "Le nom du réglage est obligatoire.");
        }

        // Une clé inconnue ne servirait à rien et ne serait jamais lue : la refuser évite
        // d'écrire une ligne que personne ne relira.
        if (Array.IndexOf(ClesReglage.Toutes, cle.Trim()) < 0)
        {
            throw new DomainException("setting_unknown_key", $"Le réglage « {cle} » n'existe pas.");
        }

        return cle.Trim();
    }

    private static string? VerifierLaValeur(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return null;
        }

        var texte = valeur.Trim();

        if (texte.Length > FieldLimits.Url)
        {
            throw new DomainException(
                "setting_value_too_long",
                $"Un réglage ne peut pas dépasser {FieldLimits.Url} caractères.");
        }

        // Le logo et les canaux sont rendus dans des attributs href et src. Un schéma
        // dangereux y aurait sa place si la valeur passait sans contrôle.
        if (Uri.TryCreate(texte, UriKind.Absolute, out var adresse)
            && adresse.Scheme is not ("http" or "https" or "mailto" or "tel"))
        {
            throw new DomainException(
                "setting_scheme_forbidden",
                "Un réglage ne peut pas désigner autre chose qu'une adresse http, https, mailto ou tel.");
        }

        return texte;
    }
}