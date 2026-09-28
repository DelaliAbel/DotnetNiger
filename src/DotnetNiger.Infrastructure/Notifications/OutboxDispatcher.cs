using DotnetNiger.Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Notifications;

/// <summary>
/// Fait tourner le relais d'expédition tant que l'application est active.
/// </summary>
/// <remarks>
/// Toute la logique d'un tour de boucle vit dans <see cref="OutboxProcessor"/>, que cette
/// classe ne fait qu'appeler en boucle. Le contexte de données étant de portée requête,
/// chaque tour ouvre sa propre portée de dépendances.
/// </remarks>
/// <param name="scopeFactory">Fabrique de portées pour obtenir un processeur par tour.</param>
/// <param name="options">Réglages du relais.</param>
/// <param name="logger">Journalisation.</param>
public sealed partial class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            LogDesactive(logger);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var treated = 0;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                treated = await processor.DrainAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
#pragma warning disable CA1031 // Le relais ne doit jamais s'arrêter sur une erreur ponctuelle.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LogErreurGenerale(logger, ex);
            }

            // Une file vide ne justifie pas de marteler la base toutes les secondes.
            if (treated == 0)
            {
                await Task.Delay(_options.IdleInterval, stoppingToken);
            }
        }
    }

    [LoggerMessage(
        EventId = 4100,
        Level = LogLevel.Information,
        Message = "Le relais de courriels est désactivé : les messages resteront en file.")]
    private static partial void LogDesactive(ILogger logger);

    [LoggerMessage(
        EventId = 4101,
        Level = LogLevel.Error,
        Message = "Le relais de courriels a rencontré une erreur et reprendra au tour suivant.")]
    private static partial void LogErreurGenerale(ILogger logger, Exception exception);
}
