using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Services;
using FluentAssertions;
using Xunit;

namespace DotnetNiger.Gateway.Tests.Services;

public class ServiceRegistryTests
{
    private static ServiceRegistry CreateEmptyRegistry()
    {
        return new ServiceRegistry(Enumerable.Empty<DownstreamServiceConfig>());
    }

    private static ServiceRegistration CreateRegistration(
        string id = "test-1", string url = "http://localhost:5001")
    {
        return new ServiceRegistration
        {
            Id = id,
            Url = url,
            Name = "Test Service",
            HealthEndpoint = "/health"
        };
    }

    [Fact]
    public void RegisterOrUpdate_AddsService()
    {
        var registry = CreateEmptyRegistry();
        var reg = CreateRegistration();

        registry.RegisterOrUpdate(reg);
        var all = registry.GetAll();
        all.Should().ContainSingle(s => s.Id == "test-1");
    }

    [Fact]
    public void RegisterOrUpdate_UpdatesExisting_WhenSameId()
    {
        var registry = CreateEmptyRegistry();
        var reg1 = new ServiceRegistration { Id = "test-1", Url = "http://old:5001", Name = "Old" };
        var reg2 = new ServiceRegistration { Id = "test-1", Url = "http://new:5001", Name = "New" };

        registry.RegisterOrUpdate(reg1);
        registry.RegisterOrUpdate(reg2);
        var all = registry.GetAll();
        all.Should().ContainSingle(s => s.Id == "test-1");
        all.Should().Contain(s => s.Name == "New");
    }

    [Fact]
    public void Unregister_RemovesService()
    {
        var registry = CreateEmptyRegistry();
        var reg = CreateRegistration();
        registry.RegisterOrUpdate(reg);

        var removed = registry.Unregister("test-1");
        removed.Should().BeTrue();
        registry.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Unregister_ReturnsFalse_WhenMissing()
    {
        var registry = CreateEmptyRegistry();
        var result = registry.Unregister("non-existent");
        result.Should().BeFalse();
    }

    [Fact]
    public void Get_ReturnsNull_WhenNotFound()
    {
        var registry = CreateEmptyRegistry();
        var result = registry.Get("missing");
        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsSnapshot_NotLiveReference()
    {
        var registry = CreateEmptyRegistry();
        var reg = CreateRegistration();
        registry.RegisterOrUpdate(reg);

        var snapshot = registry.GetAll();
        registry.Unregister("test-1");

        snapshot.Should().ContainSingle(s => s.Id == "test-1");
        registry.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void GetCombinedConfig_ReturnsRegisteredAndConfigured()
    {
        var seed = new List<DownstreamServiceConfig>
        {
            new()
            {
                Id = "seed-1", ContainerName = "seed", Port = 5000,
                DevUrl = "http://seed:5000", HealthEndpoint = "/health",
                SwaggerEndpoint = "/swagger/v1/swagger.json",
                SwaggerName = "Seed API", RoutesConfig = "ocelot.seed.routes.json"
            }
        };
        var registry = new ServiceRegistry(seed);
        var reg = new ServiceRegistration
        {
            Id = "reg-1", Url = "http://reg:5001", Name = "Registered"
        };
        registry.RegisterOrUpdate(reg);

        var config = registry.GetCombinedConfig();
        config.Should().Contain(s => s.Id == "seed-1");
        config.Should().Contain(s => s.Id == "reg-1");
    }

    [Fact]
    public void SeedServices_ArePreRegistered()
    {
        var seed = new List<DownstreamServiceConfig>
        {
            new()
            {
                Id = "seed-1", ContainerName = "my-app", Port = 8080,
                DevUrl = "http://localhost:8080", HealthEndpoint = "/health",
                SwaggerEndpoint = "/swagger/v1/swagger.json",
                SwaggerName = "My App", RoutesConfig = "ocelot.my-app.routes.json"
            }
        };
        var registry = new ServiceRegistry(seed);

        var all = registry.GetAll();
        all.Should().ContainSingle(s => s.Id == "seed-1");
        all.Should().Contain(s => s.Url == "http://localhost:8080");
    }

    [Fact]
    public async Task RegisterOrUpdate_ConcurrentAccess_DoesNotCorrupt()
    {
        var registry = CreateEmptyRegistry();
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            var reg = new ServiceRegistration
            {
                Id = $"test-{i}",
                Url = $"http://localhost:{5000 + i}",
                Name = $"Test {i}"
            };
            registry.RegisterOrUpdate(reg);
        }));

        await Task.WhenAll(tasks);
        registry.GetAll().Should().HaveCount(100);
    }
}
