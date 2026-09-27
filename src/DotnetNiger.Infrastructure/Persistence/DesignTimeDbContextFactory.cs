using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Fabrique de contexte utilisée par l'outil de ligne de commande <c>dotnet ef</c>.
/// </summary>
/// <remarks>
/// Indispensable pour générer des migrations : <c>dotnet ef</c> ne lance pas l'application
/// et n'a donc pas accès à la configuration ni au conteneur de dépendances.
/// <para>
/// Le fournisseur se règle par variable d'environnement, car les migrations d'Entity
/// Framework dépendent du provider : il faut un jeu de migrations distinct pour SQLite et
/// pour SQL Server.
/// </para>
/// <example>
/// <code>
/// dotnet ef migrations add InitialSqlServer \
///   --project src/DotnetNiger.Infrastructure \
///   --startup-project src/DotnetNiger.Web \
///   --output-dir Persistence/Migrations/SqlServer
/// </code>
/// </example>
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DotnetNigerDbContext>
{
    /// <summary>
    /// Variable d'environnement chochant le fournisseur : <c>Sqlite</c> ou <c>SqlServer</c>.
    /// </summary>
    public const string ProviderVariable = "DOTNETNIGER_DESIGN_PROVIDER";

    /// <summary>
    /// Variable d'environnement fournissant la chaîne de connexion utilisée par l'outil.
    /// </summary>
    public const string ConnectionVariable = "DOTNETNIGER_DESIGN_CONNECTION";

    private const string DefaultSqliteConnection = "Data Source=dotnetniger.design.db";

    /// <inheritdoc />
    public DotnetNigerDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var provider = Environment.GetEnvironmentVariable(ProviderVariable) is { Length: > 0 } configuredProvider
            ? configuredProvider
            : DatabaseProviders.Sqlite;

        var connectionString =
            Environment.GetEnvironmentVariable(ConnectionVariable) is { Length: > 0 } configuredConnection
                ? configuredConnection
                : DefaultSqliteConnection;

        var options = new DbContextOptionsBuilder<DotnetNigerDbContext>();
        DatabaseProviderConfiguration.Apply(options, provider, connectionString);

        return new DotnetNigerDbContext(options.Options);
    }
}
