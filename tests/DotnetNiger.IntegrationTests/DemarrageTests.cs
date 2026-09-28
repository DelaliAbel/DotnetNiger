using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie que le conteneur de l'application se construit.
/// </summary>
/// <remarks>
/// Un test d'intégration qui monte ses services à la main passe malgré un enregistrement
/// cassé : il ne voit que ce qu'il déclare lui-même. Une exception peut donc survivre
/// longtemps. C'est ce test qui l'attrape, en construisant le conteneur comme le fait
/// <c>Program</c> et en demandant au moteur de valider chaque service au montage.
/// </remarks>
public sealed class DemarrageTests
{
    [Fact]
    public void LeConteneurDeLApplicationSeConstruitEntierement()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Data Source=:memory:",
                ["Database:Provider"] = "Sqlite",
                ["Email:SiteUrl"] = "https://dotnetniger.test",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);

        // Ces deux-là sont fournis par l'hôte et par la couche web, pas par
        // l'infrastructure : on les déclare pour que le graphe assemblé ici soit le même que
        // celui de Program, et que la validation porte sur le même ensemble.
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<ICurrentUserService>(
            _ => UtilisateurFictif.Administrateur(Guid.NewGuid()));

        services.AddDotnetNigerPersistence(configuration);
        services.AddDotnetNigerIdentity(configuration);

        // ValidateOnBuild est le cœur du test : il construit chaque service enregistré et
        // signale ceux dont une dépendance est introuvable, au lieu d'attendre la première
        // requête pour s'en apercevoir.
        using var fournisseur = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        // L'unité de travail est de portée requête : elle se résout dans une portée, comme
        // le fait le pipeline à chaque appel.
        using var portee = fournisseur.CreateScope();

        Assert.NotNull(portee.ServiceProvider.GetService<IUnitOfWork>());
    }
}
