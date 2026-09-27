using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Règles de sécurité des comptes, pilotées par configuration.
/// </summary>
public sealed class AccountSecurityOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "AccountSecurity";

    /// <summary>
    /// Longueur minimale du mot de passe.
    /// </summary>
    [Range(8, 128)]
    public int MinimumPasswordLength { get; set; } = 12;

    /// <summary>
    /// Impose au moins une majuscule, une minuscule, un chiffre et un caractère non alphanumérique.
    /// </summary>
    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>
    /// Exige un chiffre dans le mot de passe.
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Impose une majuscule.
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Impose une minuscule.
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Durée de vie maximale d'un mot de passe avant changement.
    /// </summary>
    [Range(0, 730)]
    public int MaximumPasswordAgeInDays { get; set; } = 365;

    /// <summary>
    /// Nombre de tentatives échouées avant verrouillage du compte.
    /// </summary>
    [Range(1, 100)]
    public int MaxFailedAccessAttempts { get; set; } = 5;

    /// <summary>
    /// Durée du verrouillage après épuisement des tentatives.
    /// </summary>
    [Range(1, 1440)]
    public int LockoutMinutes { get; set; } = 15;

    // La durée de vie des jetons de confirmation et de réinitialisation n'est pas
    // exposée ici : Identity ne publie pas les options de ses fournisseurs de jetons.
    // Les valeurs par défaut du framework, 24 heures et 1 heure, conviennent et sont
    // volontairement conservées plutôt que dupliquées dans un réglage inopérant.
}
