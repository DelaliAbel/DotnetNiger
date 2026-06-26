using System.Collections.Concurrent;
using DotnetNiger.Gateway.Configuration;

namespace DotnetNiger.Gateway.Services;

/// <summary>Représente l'enregistrement d'un service dynamique dans le registre.</summary>
public sealed class ServiceRegistration
{
    public required string Id { get; init; }
    public required string Url { get; init; }
    public string? Name { get; init; }
    public string? HealthEndpoint { get; init; }
    public string? SwaggerEndpoint { get; init; }
    public string? ContainerName { get; init; }
    public int? Port { get; init; }
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
    public DateTime? LastHeartbeat { get; set; }
}

/// <summary>Interface du registre des services dynamiques.</summary>
public interface IServiceRegistry
{
    void RegisterOrUpdate(ServiceRegistration registration);
    bool Unregister(string serviceId);
    ServiceRegistration? Get(string serviceId);
    IReadOnlyList<ServiceRegistration> GetAll();
    IReadOnlyList<DownstreamServiceConfig> GetCombinedConfig();
}

/// <summary>Registre thread-safe des services dynamiques, combiné avec les services de démarrage (seed).</summary>
public sealed class ServiceRegistry : IServiceRegistry
{
    private readonly ConcurrentDictionary<string, ServiceRegistration> _dynamic = new();
    private readonly IReadOnlyDictionary<string, DownstreamServiceConfig> _seed;

    /// <summary>Initialise le registre avec les services de démarrage (seed).</summary>
    public ServiceRegistry(IEnumerable<DownstreamServiceConfig> seedServices)
    {
        _seed = seedServices.ToDictionary(s => s.Id);

        foreach (var seed in seedServices)
        {
            _dynamic.TryAdd(seed.Id, new ServiceRegistration
            {
                Id = seed.Id,
                Url = seed.DevUrl,
                Name = seed.SwaggerName,
                HealthEndpoint = seed.HealthEndpoint,
                SwaggerEndpoint = seed.SwaggerEndpoint,
                ContainerName = seed.ContainerName,
                Port = seed.Port
            });
        }
    }

    /// <summary>Enregistre ou met à jour un service dynamique.</summary>
    public void RegisterOrUpdate(ServiceRegistration registration)
    {
        _dynamic.AddOrUpdate(registration.Id, registration, (_, existing) =>
        {
            return new ServiceRegistration
            {
                Id = registration.Id,
                Url = registration.Url ?? existing.Url,
                Name = registration.Name ?? existing.Name,
                HealthEndpoint = registration.HealthEndpoint ?? existing.HealthEndpoint,
                SwaggerEndpoint = registration.SwaggerEndpoint ?? existing.SwaggerEndpoint,
                ContainerName = registration.ContainerName ?? existing.ContainerName,
                Port = registration.Port ?? existing.Port,
                RegisteredAt = existing.RegisteredAt,
                LastHeartbeat = DateTime.UtcNow
            };
        });
    }

    /// <summary>Désenregistre un service dynamique.</summary>
    public bool Unregister(string serviceId) => _dynamic.TryRemove(serviceId, out _);

    /// <summary>Récupère un service par son identifiant.</summary>
    public ServiceRegistration? Get(string serviceId) =>
        _dynamic.TryGetValue(serviceId, out var reg) ? reg : null;

    /// <summary>Retourne la liste de tous les services enregistrés.</summary>
    public IReadOnlyList<ServiceRegistration> GetAll() =>
        _dynamic.Values.Select(s => new ServiceRegistration
        {
            Id = s.Id,
            Url = s.Url,
            Name = s.Name,
            HealthEndpoint = s.HealthEndpoint,
            SwaggerEndpoint = s.SwaggerEndpoint,
            ContainerName = s.ContainerName,
            Port = s.Port,
            RegisteredAt = s.RegisteredAt,
            LastHeartbeat = s.LastHeartbeat
        }).ToList();

    /// <summary>Retourne la configuration combinée (seed + dynamique) pour tous les services.</summary>
    public IReadOnlyList<DownstreamServiceConfig> GetCombinedConfig()
    {
        var result = _seed.Values.Select(seed =>
        {
            if (_dynamic.TryGetValue(seed.Id, out var reg))
            {
                return new DownstreamServiceConfig
                {
                    Id = seed.Id,
                    ContainerName = reg.ContainerName ?? seed.ContainerName,
                    Port = reg.Port ?? seed.Port,
                    DevUrl = reg.Url,
                    HealthEndpoint = reg.HealthEndpoint ?? seed.HealthEndpoint,
                    SwaggerEndpoint = reg.SwaggerEndpoint ?? seed.SwaggerEndpoint,
                    SwaggerName = seed.SwaggerName,
                    RoutesConfig = seed.RoutesConfig
                };
            }
            return seed;
        }).ToList();

        foreach (var (id, reg) in _dynamic)
        {
            if (!_seed.ContainsKey(id))
            {
                result.Add(new DownstreamServiceConfig
                {
                    Id = reg.Id,
                    ContainerName = reg.ContainerName ?? reg.Id,
                    Port = reg.Port ?? 8080,
                    DevUrl = reg.Url,
                    HealthEndpoint = reg.HealthEndpoint ?? "/health",
                    SwaggerEndpoint = reg.SwaggerEndpoint ?? "/swagger/v1/swagger.json",
                    SwaggerName = reg.Name ?? $"{reg.Id} API",
                    RoutesConfig = $"ocelot.{reg.Id}.routes.json"
                });
            }
        }

        return result;
    }
}
