using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Application des règles de validation déclarées sur les formulaires de compte.
/// </summary>
/// <remarks>
/// Les pages sont rendues en HTML statique et soumettent leurs données à des points de
/// terminaison d'API minimale : la validation automatique des API minimales n'est donc pas
/// branchée, et les attributs des objets d'entrée resteraient purement décoratifs. Un membre
/// pourrait ainsi confirmer un mot de passe différent de celui choisi, ou envoyer un champ
/// vide que le navigateur avait refusé de soumettre.
/// </remarks>
public static class AccountInputValidation
{
    /// <summary>
    /// Vérifie un formulaire et renvoie le premier message d'erreur rencontré.
    /// </summary>
    /// <param name="input">Objet portant les règles de validation.</param>
    /// <returns>Le message à afficher, ou null si le formulaire est valide.</returns>
    public static string? Validate(object? input)
    {
        if (input is null)
        {
            return "Le formulaire est incomplet.";
        }

        var results = new List<ValidationResult>();
        var context = new ValidationContext(input);

        var valide = Validator.TryValidateObject(input, context, results, validateAllProperties: true);

        if (valide)
        {
            return null;
        }

        // Les messages sont ceux des attributs, rédigés en français. On ne les Accumule pas :
        // la page n'affiche qu'un bandeau, et la première erreur suffit à guider le membre.
        return results.Count > 0
            ? results[0].ErrorMessage
            : "Le formulaire est incomplet.";
    }
}
