using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Traduit les refus d'Identity, qui sont écrits en anglais dans la bibliothèque.
/// </summary>
/// <remarks>
/// Sans ce descripteur, un membre qui saisit un mot de passe trop court lit « Passwords
/// must be at least 12 characters » sur un site par ailleurs entièrement français. Le refus
/// est correct mais illisible, et la longueur annoncée se perd dans la phrase.
/// <para>
/// Seuls les messages de mot de passe sont traduits : ce sont les seuls que le membre peut
/// provoquer seul, et donc les seuls qu'il va lire. Les autres — mot de passe déjà pris,
/// adresse déverrouillée — sont rédigés par le site ou restent ignorés.
/// </para>
/// </remarks>
public sealed class FrenchIdentityErrorDescriber : IdentityErrorDescriber
{
    /// <inheritdoc />
    public override IdentityError PasswordTooShort(int length) =>
        Erreur($"Le mot de passe doit comporter au moins {length} caractères.");

    /// <inheritdoc />
    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Erreur("Le mot de passe doit comporter au moins un caractère spécial, comme ! ou ?.");

    /// <inheritdoc />
    public override IdentityError PasswordRequiresDigit() =>
        Erreur("Le mot de passe doit comporter au moins un chiffre.");

    /// <inheritdoc />
    public override IdentityError PasswordRequiresLower() =>
        Erreur("Le mot de passe doit comporter au moins une minuscule.");

    /// <inheritdoc />
    public override IdentityError PasswordRequiresUpper() =>
        Erreur("Le mot de passe doit comporter au moins une majuscule.");

    /// <inheritdoc />
    public override IdentityError PasswordMismatch() =>
        Erreur("Les deux mots de passe ne correspondent pas.");

    private static IdentityError Erreur(string description) => new() { Description = description };
}