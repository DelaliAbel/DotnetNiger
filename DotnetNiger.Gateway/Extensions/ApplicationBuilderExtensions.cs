using System.Text.Json;
using System.Text.Json.Nodes;
using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Metrics;
using DotnetNiger.Gateway.Services;
using DotnetNiger.Gateway.Middleware;
using Serilog;

namespace DotnetNiger.Gateway.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseLatencyMetricsMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/metrics/latency", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await next();
            stopwatch.Stop();

            var route = context.GetEndpoint()?.DisplayName ?? path;
            var key = $"{context.Request.Method} {route}";
            EndpointLatencyMetrics.Record(key, stopwatch.Elapsed.TotalMilliseconds, context.Response.StatusCode);
        });

        return app;
    }

    public static IApplicationBuilder UseClientIdResolutionMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var resolvedClientId = context.Request.Headers["ClientId"].FirstOrDefault()
                                   ?? context.Request.Headers["Oc-Client"].FirstOrDefault()
                                   ?? context.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                                   ?? "unknown-client";

            if (!context.Request.Headers.ContainsKey("ClientId"))
                context.Request.Headers["ClientId"] = resolvedClientId;

            if (!context.Request.Headers.ContainsKey("Oc-Client"))
                context.Request.Headers["Oc-Client"] = resolvedClientId;

            await next.Invoke();
        });

        return app;
    }

    public static IApplicationBuilder UseRequestTracingMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Request-ID"] = requestId;
            Log.Information("→ {Method} {Path}", context.Request.Method, context.Request.Path);
            await next.Invoke();
            Log.Information("← {StatusCode}", context.Response.StatusCode);
        });

        return app;
    }

    public static IApplicationBuilder UseExternalServiceProxy(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExternalServiceProxyMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseCustomSwaggerMergeMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                var isMergedSwaggerPath = context.Request.Path.Equals("/swagger/docs/v1/all", StringComparison.OrdinalIgnoreCase)
                    || context.Request.Path.Equals("/swagger/v1/swagger.json", StringComparison.OrdinalIgnoreCase);

                if (!isMergedSwaggerPath)
                {
                    await next(context);
                    return;
                }

                var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                var registry = context.RequestServices.GetRequiredService<IServiceRegistry>();
                var services = registry.GetCombinedConfig();

                var swaggerJsons = await Task.WhenAll(
                    services.Select(s => FetchSwaggerJsonAsync(factory, s, context.RequestAborted)));

                var validResults = swaggerJsons.Where(r => r.json != null).ToList();

                if (validResults.Count == 0)
                {
                    Log.Warning("Swagger aggregation failed: all downstream documents unavailable");
                    context.Response.StatusCode = 503;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\":\"Downstream swagger documents are unavailable\"}");
                    return;
                }

                var merged = MergeSwaggerDocuments(validResults, context.Request.Scheme, context.Request.Host);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(merged);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Swagger merge middleware error");
                await next(context);
            }
        });

        return app;
    }

    public static IApplicationBuilder MapGatewayHealthEndpoints(this IApplicationBuilder app)
    {
        app.Map("/health", healthApp =>
        {
            healthApp.Run(async (HttpContext context) =>
            {
                var path = context.Request.Path.Value ?? "";
                var response = context.Response;
                response.ContentType = "application/json";

                var registry = context.RequestServices.GetRequiredService<IServiceRegistry>();
                var services = registry.GetCombinedConfig();

                switch (path)
                {
                    case "":
                    case "/":
                    {
                        await response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            status = "Healthy",
                            service = "DotnetNiger.Gateway",
                            timestamp = DateTime.UtcNow
                        }));
                        return;
                    }
                    case "/downstream":
                    {
                        var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var ct = context.RequestAborted;
                        var results = await Task.WhenAll(
                            services.Select(s => CheckDownstreamAsync(factory, s, ct)));

                        var allHealthy = results.All(r => r.IsHealthy);
                        response.StatusCode = allHealthy ? 200 : 503;

                        await response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            status = allHealthy ? "Healthy" : "Degraded",
                            service = "DotnetNiger.Gateway",
                            timestamp = DateTime.UtcNow,
                            downstream = results.ToDictionary(r => r.ServiceId, r => new
                            {
                                url = r.Url,
                                isHealthy = r.IsHealthy,
                                statusCode = r.StatusCode,
                                reason = r.Reason
                            })
                        }));
                        return;
                    }
                    case "/ready":
                    {
                        var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var ct = context.RequestAborted;
                        var results = await Task.WhenAll(
                            services.Select(s => CheckDownstreamAsync(factory, s, ct)));

                        response.StatusCode = results.All(r => r.IsHealthy) ? 200 : 503;
                        return;
                    }
                    case "/services":
                    {
                        await response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            gateway = "DotnetNiger.Gateway",
                            timestamp = DateTime.UtcNow,
                            services = services.Select(s => new
                            {
                                id = s.Id,
                                name = s.SwaggerName,
                                devUrl = s.DevUrl,
                                containerName = s.ContainerName,
                                port = s.Port,
                                healthEndpoint = s.HealthEndpoint,
                                swaggerEndpoint = s.SwaggerEndpoint,
                                routesConfig = s.RoutesConfig
                            })
                        }));
                        return;
                    }
                    default:
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }
                }
            });
        });

        app.Map("/metrics/latency", metricsApp =>
        {
            metricsApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(EndpointLatencyMetrics.GetSnapshot()));
            });
        });

        return app;
    }

    private static async Task<(string? json, string serviceId)> FetchSwaggerJsonAsync(
        IHttpClientFactory factory, DownstreamServiceConfig service, CancellationToken ct)
    {
        try
        {
            var url = $"{service.DevUrl.TrimEnd('/')}{service.SwaggerEndpoint}";
            using var client = factory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(4);
            var json = await client.GetStringAsync(url, ct);
            return (json, service.Id);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to fetch swagger from {Service}", service.Id);
            return (null, service.Id);
        }
    }

    private static string MergeSwaggerDocuments(
        List<(string? json, string serviceId)> results,
        string scheme,
        HostString host)
    {
        JsonObject? merged = null;

        foreach (var (json, _) in results)
        {
            if (json == null) continue;

            var doc = JsonNode.Parse(json)?.AsObject();
            if (doc == null) continue;

            if (merged == null)
            {
                merged = doc;
                if (merged["info"] is JsonObject info)
                    info["title"] = "DotnetNiger - All APIs";
                continue;
            }

            var paths = merged["paths"]?.AsObject() ?? new JsonObject();
            if (doc["paths"] is JsonObject docPaths)
                foreach (var p in docPaths)
                    paths[p.Key] = p.Value?.DeepClone();
            merged["paths"] = paths;

            var mergedSchemas = merged["components"]?["schemas"]?.AsObject() ?? new JsonObject();
            if (doc["components"]?["schemas"] is JsonObject docSchemas)
                foreach (var s in docSchemas)
                    if (!mergedSchemas.ContainsKey(s.Key))
                        mergedSchemas[s.Key] = s.Value?.DeepClone();

            merged["components"] = new JsonObject { ["schemas"] = mergedSchemas };
        }

        merged ??= new JsonObject();

        merged["servers"] = new JsonArray
        {
            new JsonObject { ["url"] = $"{scheme}://{host}" }
        };

        return merged.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static async Task<DownstreamHealthResult> CheckDownstreamAsync(
        IHttpClientFactory factory, DownstreamServiceConfig service, CancellationToken ct)
    {
        try
        {
            var url = $"{service.DevUrl.TrimEnd('/')}{service.HealthEndpoint}";
            using var client = factory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            using var response = await client.GetAsync(url, ct);

            return new DownstreamHealthResult(
                service.Id,
                url,
                response.IsSuccessStatusCode,
                (int)response.StatusCode,
                response.ReasonPhrase);
        }
        catch (Exception ex)
        {
            return new DownstreamHealthResult(
                service.Id,
                $"{service.DevUrl}{service.HealthEndpoint}",
                false,
                503,
                ex.Message);
        }
    }

    private readonly record struct DownstreamHealthResult(
        string ServiceId, string Url, bool IsHealthy, int StatusCode, string? Reason);
}
