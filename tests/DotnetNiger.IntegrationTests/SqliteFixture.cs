using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Infrastructure.Persistence.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Crée un <see cref="DotnetNigerDbContext"/> sur une base SQLite temporaire, avec le
/// même câblage que l'application : audit automatique et soft delete.
/// </summary>
public sealed class SqliteFixture : IDisposable
{
    private readonly string _cheminBase;

    public SqliteFixture()
    {
        _cheminBase = Path.Combine(
            Path.GetTempPath(),
            $"dotnetniger-tests-{Guid.NewGuid():N}.db");

        var horloge = new HorlogeFixe(DateTimeOffset.Parse(
            "2026-09-27T10:15:30+00:00",
            System.Globalization.CultureInfo.InvariantCulture));

        Context = BuildContext(horloge, UtilisateurFictif.Anonyme);

        Context.Database.EnsureCreated();
    }

    public DotnetNigerDbContext Context { get; }

    /// <summary>
    /// Construit un contexte neuf mais branché sur la même base, avec un utilisateur
    /// donné, pour simuler une autre requête.
    /// </summary>
    public DotnetNigerDbContext CreateContextFor(TimeProvider horloge, ICurrentUserService utilisateur)
    {
        return BuildContext(horloge, utilisateur);
    }

    private DotnetNigerDbContext BuildContext(
        TimeProvider horloge,
        ICurrentUserService utilisateur)
    {
        var options = new DbContextOptionsBuilder<DotnetNigerDbContext>()
            .UseSqlite($"Data Source={_cheminBase}")
            .AddInterceptors(new AuditSaveChangesInterceptor(horloge, utilisateur))
            .Options;

        return new DotnetNigerDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        SqliteConnection.ClearAllPools();

        foreach (var fichier in new[] { _cheminBase, _cheminBase + "-shm", _cheminBase + "-wal" })
        {
            if (File.Exists(fichier))
            {
                File.Delete(fichier);
            }
        }
    }

    /// <summary>
    /// Horloge figée, pour que les dates d'audit soient vérifiables.
    /// </summary>
    private sealed class HorlogeFixe(DateTimeOffset instant) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => instant;
    }
}
