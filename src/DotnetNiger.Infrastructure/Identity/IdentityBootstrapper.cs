using DotnetNiger.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Paramètres de création du compte administrateur initial.
/// </summary>
public sealed class AdminBootstrapOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "AdminBootstrap";

    /// <summary>
    /// Adresse du compte à créer. Sans valeur, aucun compte n'est créé : un site neuf
    /// démarre donc sans administrateur plutôt qu'avec un accès ouvert.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Mot de passe initial. À fournir par un secret du serveur ; jamais dans un fichier
    /// versionné.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Prénom affiché du compte.
    /// </summary>
    public string DisplayName { get; set; } = "Administration";

    /// <summary>
    /// Vrai si un compte doit être créé au démarrage.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Email)
        && !string.IsNullOrWhiteSpace(Password);
}

/// <summary>
/// Création des rôles et du premier compte administrateur.
/// </summary>
public sealed partial class IdentityBootstrapper(
    IServiceProvider services,
    IConfiguration configuration,
    ILogger<IdentityBootstrapper> logger)
{
    /// <summary>
    /// Crée les rôles manquants, puis le compte administrateur initial s'il est configuré.
    /// </summary>
    /// <returns>Tâche complète une fois la base à jour.</returns>
    /// <remarks>
    /// L'opération est idempotente : elle peut s'exécuter à chaque démarrage sans effet
    /// de bord. Le compte administrateur n'est créé que si la section de configuration est
    /// complète ; un mot de passe manquant est un refus, pas une invention.
    /// </remarks>
    public async Task InitializeAsync()
    {
        await using var scope = services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleAsync(roleManager, RoleNames.Admin);
        await EnsureRoleAsync(roleManager, RoleNames.Member);

        var options = configuration
            .GetSection(AdminBootstrapOptions.SectionName)
            .Get<AdminBootstrapOptions>() ?? new AdminBootstrapOptions();

        if (!options.IsConfigured)
        {
            return;
        }

        // `IsConfigured` garantit les deux valeurs ; la garde sert seulement à satisfaire
        // l'analyseur de nullabilité, qui ne connaît pas cette propriété calculée.

        var email = options.Email!.Trim();
        var existing = await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            // Le compte existe déjà : le démarrage ne remet jamais un mot de passe à zéro,
            // ce qui verrouillerait l'accès au site à chaque redéploiement.
            return;
        }

        var admin = new ApplicationUser(email);
        var creation = await userManager.CreateAsync(admin, options.Password!);

        if (!creation.Succeeded)
        {
            LogCreationImpossiblee(
                logger,
                email,
                string.Join(" ; ", creation.Errors.Select(erreur => erreur.Description)));

            return;
        }

        await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        await userManager.AddToRoleAsync(admin, RoleNames.Member);

        // L'adresse a été fournie par l'exploitation et non par un visiteur : elle n'a pas
        // à être confirmée par un lien envoyé à une boîte qui n'existe peut-être pas encore.
        admin.EmailConfirmed = true;
        await userManager.UpdateAsync(admin);

        LogAdministrateurCree(logger, email);
    }

    private static async Task EnsureRoleAsync(RoleManager<ApplicationRole> roleManager, string role)
    {
        if (await roleManager.RoleExistsAsync(role))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new ApplicationRole(role));

        if (!result.Succeeded)
        {
            // Un rôle déjà présent sous une autre casse n'est pas un échec.
            return;
        }
    }

    [LoggerMessage(
        EventId = 30,
        Level = LogLevel.Information,
        Message = "Compte administrateur initial créé pour {Email}.")]
    private static partial void LogAdministrateurCree(ILogger logger, string email);

    [LoggerMessage(
        EventId = 31,
        Level = LogLevel.Error,
        Message = "Le compte administrateur {Email} n'a pas pu être créé : {Motif}")]
    private static partial void LogCreationImpossiblee(ILogger logger, string email, string motif);
}
