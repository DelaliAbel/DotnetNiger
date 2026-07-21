using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Api.Services.User;

public class DeletionProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeletionProcessorService> _logger;

    public DeletionProcessorService(IServiceScopeFactory scopeFactory, ILogger<DeletionProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
                await accountService.ProcessPendingDeletionsAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors du traitement des suppressions différées");
            }
        }
    }
}
