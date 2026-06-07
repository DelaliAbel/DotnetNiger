using System.Net;
using DotnetNiger.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace DotnetNiger.Gateway.Tests.Services;

public class ExternalServiceHealthServiceTests
{
    private static (Mock<IServiceScopeFactory>, Mock<IConfiguration>,
                    Mock<IHttpClientFactory>, MemoryCache) CreateMocks()
    {
        var scopeFactory = new Mock<IServiceScopeFactory>();

        var config = new Mock<IConfiguration>();
        config.Setup(c => c["DeveloperPortal:IdentityBaseUrl"])
            .Returns("http://localhost:5075");
        config.Setup(c => c["DeveloperPortal:HealthCheckIntervalSeconds"])
            .Returns("1");
        config.Setup(c => c["DeveloperPortal:MaxHealthCheckFailures"])
            .Returns("3");
        config.Setup(c => c["DeveloperPortal:InternalApiKey"])
            .Returns("test-key");

        var cache = new MemoryCache(new MemoryCacheOptions());
        var httpFactory = new Mock<IHttpClientFactory>();

        return (scopeFactory, config, httpFactory, cache);
    }

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var (scopeFactory, config, httpFactory, cache) = CreateMocks();

        var act = () => new ExternalServiceHealthService(
            scopeFactory.Object,
            config.Object,
            httpFactory.Object,
            cache);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task StartAndStop_WithEmptyServiceList_DoesNotThrow()
    {
        var (scopeFactory, config, httpFactory, cache) = CreateMocks();

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "[]", System.Text.Encoding.UTF8, "application/json")
            });
        var client = new HttpClient(handler);
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var service = new ExternalServiceHealthService(
            scopeFactory.Object,
            config.Object,
            httpFactory.Object,
            cache);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAndStop_WhenIdentityApiFails_DoesNotThrow()
    {
        var (scopeFactory, config, httpFactory, cache) = CreateMocks();

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var client = new HttpClient(handler);
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var service = new ExternalServiceHealthService(
            scopeFactory.Object,
            config.Object,
            httpFactory.Object,
            cache);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAndStop_WhenIdentityApiThrows_DoesNotThrow()
    {
        var (scopeFactory, config, httpFactory, cache) = CreateMocks();

        var handler = new FakeHttpMessageHandler(
            () => throw new HttpRequestException("Connection refused"));
        var client = new HttpClient(handler);
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var service = new ExternalServiceHealthService(
            scopeFactory.Object,
            config.Object,
            httpFactory.Object,
            cache);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);
    }
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpResponseMessage> _responseFactory;

    public FakeHttpMessageHandler(HttpResponseMessage response)
        : this(() => response) { }

    public FakeHttpMessageHandler(Func<HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_responseFactory());
    }
}
