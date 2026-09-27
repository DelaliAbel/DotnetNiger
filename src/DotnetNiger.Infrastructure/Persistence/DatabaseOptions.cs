namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Fournisseurs de base de données pris en charge.
/// </summary>
public static class DatabaseProviders
{
    /// <summary>
    /// SQLite, utilisé en développement local et pour les tests rapides.
    /// </summary>
    public const string Sqlite = "Sqlite";

    /// <summary>
    /// SQL Server, utilisé en production sur MonsterASP.NET.
    /// </summary>
    public const string SqlServer = "SqlServer";
}

/// <summary>
/// Options de persistance lues depuis la section <c>Database</c> de la configuration.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Fournisseur à utiliser. Voir <see cref="DatabaseProviders"/>.
    /// </summary>
    public string Provider { get; set; } = DatabaseProviders.Sqlite;

    /// <summary>
    /// Manière d'initialiser le schéma au démarrage.
    /// </summary>
    public DatabaseInitialization Initialization { get; set; } = DatabaseInitialization.None;
}

/// <summary>
/// Stratégies d'initialisation du schéma.
/// </summary>
public enum DatabaseInitialization
{
    /// <summary>
    /// Ne rien faire au démarrage. La migration a déjà été appliquée lors du déploiement,
    /// ou le schéma est géré ailleurs. C'est le mode attendu en production.
    /// </summary>
    None = 0,

    /// <summary>
    /// Créer le schéma directement depuis le modèle si la base est vide, sans lire les
    /// migrations. Utilisé en développement local et en test, où la base est jetable.
    /// </summary>
    /// <remarks>
    /// <c>EnsureCreated</c> et <c>Migrate</c> ne se combinent pas : la première ignore
    /// l'historique des migrations. D'où le choix d'un seul jeu de migrations, généré pour
    /// SQL Server, qui est la cible de production. SQLite n'a pas besoin de migrations
    /// puisqu'aucune donnée n'y est conservée.
    /// </remarks>
    EnsureCreated = 1,

    /// <summary>
    /// Appliquer les migrations en attente. Utilisé en intégration continue et lors d'un
    /// déploiement contrôlé, avec sauvegarde préalable de la base.
    /// </summary>
    Migrate = 2,
}
