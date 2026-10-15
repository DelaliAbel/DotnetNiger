using System.Data;
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
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(migrationsAssembly);

                // Sur une base distante, une connexion interrompue est une panne passagère :
                // sans reprise, elle devient une erreur 500. Trois tentatives, espacées de
                // dix secondes, couvrent le cas le plus courant — une connexion que le
                // réseau ou le serveur a lâchée.
                //
                // Contrepartie : EF refuse alors une transaction ouverte à la main. Les
                // deux transactions du site passent par IUnitOfWork.ExecuteInTransactionAsync,
                // qui passe par la stratégie d'exécution.
                sql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            return;
        }

        throw new InvalidOperationException(
            $"Fournisseur de base de données inconnu : « {provider} ». Valeurs acceptées : " +
            $"{DatabaseProviders.Sqlite}, {DatabaseProviders.SqlServer}.");
    }
}
