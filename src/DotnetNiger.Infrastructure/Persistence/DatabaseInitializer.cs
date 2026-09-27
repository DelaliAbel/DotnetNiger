using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Initialisation du schéma au démarrage de l'application.
/// </summary>
public static partial class DatabaseInitializer
{
    /// <summary>
    /// Applique la stratégie d'initialisation décrite par la configuration.
    /// </summary>
    /// <param name="services">Conteneur de services de l'application.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>Tâche représentant l'initialisation.</returns>
    /// <remarks>
    /// Appelé dans une portée dédiée : le contexte de persistance est de durée
    /// <c>scoped</c> et ne peut pas être résolu depuis la racine de l'application.
    /// </remarks>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var initialization = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>()?.Initialization ?? DatabaseInitialization.None;

        if (initialization == DatabaseInitialization.None)
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DotnetNigerDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitializer));

        LogSchemaInitialization(
            logger,
            initialization,
            context.Database.ProviderName ?? " fournisseur inconnu");

        if (context.Database.IsSqlite())
        {
            EnsureSqliteDirectoryExists(context.Database.GetConnectionString());
        }

        if (initialization == DatabaseInitialization.EnsureCreated)
        {
            if (await context.Database.EnsureCreatedAsync())
            {
                LogSchemaCreated(logger);
            }
            else
            {
                LogSchemaAlreadyPresent(logger);
            }

            return;
        }

        await context.Database.MigrateAsync();
        LogMigrationsApplied(logger);
    }

    /// <summary>
    /// Crée le dossier du fichier SQLite s'il n'existe pas. SQLite échoue avec
    /// « unable to open database file » si un segment de chemin est absent, et ne
    /// crée que le fichier, jamais les répertoires.
    /// </summary>
    /// <param name="connectionString">Chaîne de connexion utilisée par l'application.</param>
    private static void EnsureSqliteDirectoryExists(string? connectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

        if (string.IsNullOrWhiteSpace(dataSource)
            || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Initialisation du schéma : {Initialization} sur {Provider}.")]
    private static partial void LogSchemaInitialization(
        ILogger logger,
        DatabaseInitialization initialization,
        string provider);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Schéma créé à partir du modèle.")]
    private static partial void LogSchemaCreated(ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Le schéma existe déjà, aucune création nécessaire.")]
    private static partial void LogSchemaAlreadyPresent(ILogger logger);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Migrations en attente appliquées.")]
    private static partial void LogMigrationsApplied(ILogger logger);
}
