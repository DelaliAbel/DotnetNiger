using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Notifications;
using DotnetNiger.Infrastructure.Content;
using DotnetNiger.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DotnetNiger.Infrastructure.Queries;

namespace DotnetNiger.Infrastructure;

/// <summary>
/// Enregistrement de la persistance dans le conteneur de dépendances.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Déclare le contexte, les dépôts et l'unité de travail.
    /// </summary>
    /// <param name="services">Conteneur de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>Le même conteneur, pour permettre le chaînage.</returns>
    /// <exception cref="InvalidOperationException">La chaîne de connexion par défaut est absente.</exception>
    /// <remarks>
    /// Le fournisseur est choisi par configuration, jamais par le code : passer de SQLite à
    /// SQL Server se fait en changeant <c>Database:Provider</c> et la chaîne de connexion,
    /// sans toucher une ligne de code.
    /// </remarks>
    public static IServiceCollection AddDotnetNigerPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "La chaîne de connexion « ConnectionStrings:Default » est absente de la configuration.");
        }

        services.TryAddSingletonProvider();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMemberDirectory, MemberDirectory>();

        // Les lectures du site public. Les contrôleurs d'API et les pages Blazor
        // partagent ces services : une ressource retirée de la base disparaît donc des
        // deux côtés au même endroit, et jamais d'un seul.
        services.AddScoped<ArticleQueries>();
        services.AddScoped<EvenementQueries>();
        services.AddScoped<CatalogueQueries>();
        services.AddScoped<DepotRessources>();
        services.AddScoped<DepotPhotosProfil>();
        services.AddScoped<DepotImagesContenu>();
        services.AddScoped<DepotImagesContenu>();
        services.AddScoped<IEmailDispatcher, EmailDispatcher>();
        services.AddScoped<INewsletterDiffusion, NewsletterDiffusion>();
        services.AddScoped<IOutbox, Outbox>();
        services.AddScoped<OutboxProcessor>();
        // Rubriques du blog amorcées au démarrage, de façon idempotente : elles sont
        // des données, la table doit donc être peuplée sur une base neuve.
        services.AddSingleton<RubriqueBootstrapper>();

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddHostedService<OutboxDispatcher>();

        services.AddDbContext<DotnetNigerDbContext>((serviceProvider, builder) =>
        {
            DatabaseProviderConfiguration.Apply(builder, options.Provider, connectionString);
            builder.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        return services;
    }

    private static void TryAddSingletonProvider(this IServiceCollection services)
    {
        // TimeProvider est un service singleton : s'il a déjà été enregistré, par exemple
        // par un test, on ne l'écrase pas.
        if (services.All(descriptor => descriptor.ServiceType != typeof(TimeProvider)))
        {
            services.AddSingleton(TimeProvider.System);
        }
    }
}
