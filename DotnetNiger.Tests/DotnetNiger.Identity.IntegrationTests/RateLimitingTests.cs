using System.Net;
using System.Threading.Tasks;
using DotnetNiger.Identity.IntegrationTests;
using Xunit;

namespace DotnetNiger.Identity.IntegrationTests;

public class RateLimitingTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(IdentityWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthEndpoints_RateLimitIsApplied()
    {
        // Arrange: Clear any existing rate limit state by making a few requests to other endpoints
        // Act: Make multiple rapid requests to an auth endpoint (forgot password)
        var tasks = new Task<HttpResponseMessage>[15]; // Try 15 requests (limit is 10 per minute)
        
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = _client.PostAsync("/Account/ForgotPassword", 
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Email", "test" + i + "@example.com")
                }));
        }
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert: Most should succeed (under limit), some should be rate limited (429)
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var rateLimitedCount = responses.Count(r => r.StatusCode == (HttpStatusCode)429);
        
        // We should see some rate limiting (exact count depends on test timing)
        Assert.True(rateLimitedCount > 0, "Expected at least some requests to be rate limited (429)");
        Assert.True(successCount > 0, "Expected some requests to succeed");
    }
}