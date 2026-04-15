// Integration externe Identity: AvatarCleanupService
using Azure.Storage.Blobs;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Infrastructure.External;

// Nettoyage periodique des avatars orphelins.
// Utilise IOptionsMonitor pour supporter le toggle dynamique via admin settings.
public class AvatarCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<FileUploadOptions> _optionsMonitor;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AvatarCleanupService> _logger;

    public AvatarCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<FileUploadOptions> optionsMonitor,
        IWebHostEnvironment environment,
        ILogger<AvatarCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _environment = environment;
        _logger = logger;
    }

    private FileUploadOptions Options => _optionsMonitor.CurrentValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AvatarCleanupService started. CleanupEnabled={Enabled}", Options.CleanupEnabled);

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = Options;
            var intervalMinutes = Math.Max(5, options.CleanupIntervalMinutes);
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            if (options.CleanupEnabled)
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                    _logger.LogInformation("Avatar cleanup cycle completed");
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Avatar cleanup failed");
                }
            }
            else
            {
                _logger.LogDebug("Avatar cleanup is disabled, skipping cycle");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var options = Options;
        if (string.Equals(options.Provider, "Azure", StringComparison.OrdinalIgnoreCase))
        {
            await CleanupAzureAsync(cancellationToken);
            return;
        }

        await CleanupLocalAsync(cancellationToken);
    }

    private async Task CleanupLocalAsync(CancellationToken cancellationToken)
    {
        var options = Options;
        var rootPath = Path.Combine(_environment.ContentRootPath, options.RootPath);
        var avatarsRoot = Path.Combine(rootPath, "avatars");
        if (!Directory.Exists(avatarsRoot))
        {
            return;
        }

        var referenced = await GetReferencedAvatarSetAsync(cancellationToken, ExtractRelativePath);
        var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, options.CleanupOrphanDays));

        foreach (var file in Directory.EnumerateFiles(avatarsRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(rootPath, file).Replace('\\', '/');
            if (referenced.Contains(relative))
            {
                continue;
            }

            var lastWrite = File.GetLastWriteTimeUtc(file);
            if (lastWrite > cutoff)
            {
                continue;
            }

            try
            {
                File.Delete(file);
                _logger.LogInformation("Deleted orphan avatar file {File}", file);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to delete avatar file {File}", file);
            }
        }
    }

    private async Task CleanupAzureAsync(CancellationToken cancellationToken)
    {
        var options = Options;
        if (string.IsNullOrWhiteSpace(options.Azure.ConnectionString) || string.IsNullOrWhiteSpace(options.Azure.Container))
        {
            return;
        }

        var referenced = await GetReferencedAvatarSetAsync(cancellationToken, ExtractBlobName);
        var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, options.CleanupOrphanDays));
        var containerClient = new BlobContainerClient(options.Azure.ConnectionString, options.Azure.Container);

        await foreach (var blob in containerClient.GetBlobsAsync(prefix: "avatars/", cancellationToken: cancellationToken))
        {
            if (referenced.Contains(blob.Name))
            {
                continue;
            }

            if (blob.Properties.LastModified.HasValue && blob.Properties.LastModified.Value.UtcDateTime > cutoff)
            {
                continue;
            }

            try
            {
                await containerClient.DeleteBlobIfExistsAsync(blob.Name, cancellationToken: cancellationToken);
                _logger.LogInformation("Deleted orphan blob {Blob}", blob.Name);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to delete blob {Blob}", blob.Name);
            }
        }
    }

    private async Task<HashSet<string>> GetReferencedAvatarSetAsync(
        CancellationToken cancellationToken,
        Func<string, string?> extractor)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DotnetNigerIdentityDbContext>();
        var urls = await dbContext.Users
            .AsNoTracking()
            .Select(user => user.AvatarUrl)
            .ToListAsync(cancellationToken);

        var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in urls)
        {
            var relative = extractor(url);
            if (!string.IsNullOrWhiteSpace(relative))
            {
                referenced.Add(relative.Replace('\\', '/'));
            }
        }

        return referenced;
    }

    private string? ExtractRelativePath(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return null;
        }

        var options = Options;
        var basePath = NormalizePublicBasePath(options.PublicBasePath);
        var rawPath = fileUrl;
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            rawPath = uri.AbsolutePath;
        }

        if (!rawPath.StartsWith('/'))
        {
            rawPath = "/" + rawPath;
        }

        if (!rawPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return rawPath[basePath.Length..].TrimStart('/');
    }

    private string? ExtractBlobName(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return null;
        }

        var options = Options;
        var publicBaseUrl = options.Azure.PublicBaseUrl;
        if (!string.IsNullOrWhiteSpace(publicBaseUrl) &&
            fileUrl.StartsWith(publicBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return fileUrl[publicBaseUrl.TrimEnd('/').Length..].TrimStart('/');
        }

        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            // Extraire le chemin relatif depuis l'URI absolue.
            return uri.AbsolutePath.TrimStart('/');
        }

        return fileUrl.TrimStart('/');
    }

    private static string NormalizePublicBasePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/uploads";
        }

        var trimmed = path.Trim();
        if (!trimmed.StartsWith('/'))
        {
            trimmed = "/" + trimmed;
        }

        return trimmed.TrimEnd('/');
    }
}
