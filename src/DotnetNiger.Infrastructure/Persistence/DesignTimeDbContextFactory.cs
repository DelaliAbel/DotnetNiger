using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Construit un <see cref="DotnetNigerDbContext"/> pour les outils Entity Framework.
/// </summary>
/// <remarks>
/// L'outil <c>dotnet ef</c> lance l'application pour obtenir son contexte. Ce faisant, il
/// n'applique que les variables d'environnement de <c>launchSettings.json</c> : la
/// configuration réelle du poste — et donc le fournisseur de base de données — ne lui
/// parvient pas, et une migration peut se retrouver générée pour le mauvais fournisseur.
/// Une migration SQL Server rejouée sur SQLite, ou l'inverse, échoue au démarrage, très
/// loin de sa cause.
/// <para>
/// Cette fabrique lit la même configuration que l'application, dans le même ordre de
/// priorité, et appelle le même sélecteur de fournisseur. Une variable d'environnement
/// posée dans le shell — <c>Database__Provider=SqlServer</c> — suffit alors à générer une
/// migration pour la bonne cible, sans aller et venir dans les fichiers de configuration.
/// </para>
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DotnetNigerDbContext>
{
    /// <summary>
    /// Construit le contexte de conception.
    /// </summary>
    /// <param name="args">Arguments de l'outil, ignorés.</param>
    /// <returns>Un contexte configuré, jamais connecté.</returns>
    /// <remarks>
    /// Aucune connexion n'est ouverte : la fabrique sert à lire le modèle, et se connecter
    /// ici ferait échouer <c>migrations add</c> sur un poste dont la base est hors service.
    /// </remarks>
    public DotnetNigerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(RepertoireDeConfiguration())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environnement()}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "La chaîne de connexion « ConnectionStrings:Default » est absente de la configuration.");

        var builder = new DbContextOptionsBuilder<DotnetNigerDbContext>();
        DatabaseProviderConfiguration.Apply(builder, options.Provider, connectionString);

        return new DotnetNigerDbContext(builder.Options);
    }

    /// <summary>
    /// Nom de l'environnement de configuration, pris dans la variable usuelle de l'hôte.
    /// </summary>
    private static string Environnement() =>
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environments.Production;

    /// <summary>
    /// Remonte depuis le répertoire courant jusqu'à la configuration de l'application.
    /// </summary>
    /// <returns>Le dossier contenant <c>appsettings.json</c>.</returns>
    /// <remarks>
    /// L'outil part du projet de démarrage, mais sa conception achanged de répertoire
    /// plusieurs fois : la recherche évite de dépendre d'un chemin codé en dur, qui
    /// casserait dès que la solution serait déplacée.
    /// </remarks>
    private static string RepertoireDeConfiguration()
    {
        var candidat = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (candidat is not null)
        {
            if (File.Exists(Path.Combine(candidat.FullName, "appsettings.json")))
            {
                return candidat.FullName;
            }

            if (Directory.Exists(Path.Combine(candidat.FullName, "src")))
            {
                var application = Path.Combine(candidat.FullName, "src", "DotnetNiger.Web");

                if (File.Exists(Path.Combine(application, "appsettings.json")))
                {
                    return application;
                }
            }

            candidat = candidat.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
