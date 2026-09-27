using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Application du fournisseur de base de données sur un constructeur d'options.
/// </summary>
/// <remarks>
/// Partagé par l'injection de dépendances et par la fabrique de temps de conception,
/// afin que <c>dotnet ef</c> et l'application utilize exactement le même provider.
/// </remarks>
internal static class DatabaseProviderConfiguration
{
    /// <summary>
    /// Configure le fournisseur demandé.
    /// </summary>
    /// <param name="options">Constructeur d'options à configurer.</param>
    /// <param name="provider">Nom du fournisseur.</param>
    /// <param name="connectionString">Chaîne de connexion.</param>
    /// <exception cref="InvalidOperationException">Le fournisseur est inconnu.</exception>
    public static void Apply(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        var migrationsAssembly = typeof(DotnetNigerDbContext).Assembly.FullName;

        if (string.Equals(provider, DatabaseProviders.Sqlite, StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(migrationsAssembly));
            return;
        }

        if (string.Equals(provider, DatabaseProviders.SqlServer, StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            return;
        }

        throw new InvalidOperationException(
            $"Fournisseur de base de données inconnu : « {provider} ». Valeurs acceptées : " +
            $"{DatabaseProviders.Sqlite}, {DatabaseProviders.SqlServer}.");
    }
}
