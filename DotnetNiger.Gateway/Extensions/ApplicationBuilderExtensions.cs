using System.Text.Json;
using System.Text.Json.Nodes;
using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Metrics;
using DotnetNiger.Gateway.Services;
using DotnetNiger.Gateway.Middleware;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Gateway.Extensions;

/// <summary>Extensions pour configurer le pipeline middleware du Gateway.</summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>Ajoute le middleware de mesure de latence des endpoints.</summary>
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

    /// <summary>Ajoute le middleware qui résout et injecte le ClientId dans les en-têtes de la requête.</summary>
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

    /// <summary>Ajoute le middleware de traçage des requêtes avec journalisation et identifiant unique.</summary>
    public static IApplicationBuilder UseRequestTracingMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Request-ID"] = requestId;
            var logger = context.RequestServices.GetRequiredService<ILogger<HttpContext>>();
            logger.LogInformation("→ {Method} {Path}", context.Request.Method, context.Request.Path);
            await next.Invoke();
            logger.LogInformation("← {StatusCode}", context.Response.StatusCode);
        });

        return app;
    }

    /// <summary>Ajoute les en-têtes de sécurité HTTP (X-Content-Type-Options, X-Frame-Options, HSTS, etc.).</summary>
    public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            // Content-Security-Policy header is not set here because this is an API gateway.
            // CSP is only meaningful for HTML pages, not for JSON API responses.
            // If needed in the future, apply it conditionally based on response content-type.

            if (!context.Response.Headers.ContainsKey("Strict-Transport-Security"))
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            await next();
        });

        return app;
    }

    /// <summary>Ajoute le middleware de proxy pour les services externes.</summary>
    public static IApplicationBuilder UseExternalServiceProxy(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExternalServiceProxyMiddleware>();
        return app;
    }

    /// <summary>Ajoute le middleware de gestion des cookies d'authentification.</summary>
    public static IApplicationBuilder UseTokenCookieMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<TokenCookieMiddleware>();
        return app;
    }

    /// <summary>Ajoute le middleware de génération des balises Open Graph pour les crawlers sociaux.</summary>
    public static IApplicationBuilder UseOpenGraphMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<OpenGraphMiddleware>();
        return app;
    }

    /// <summary>Ajoute le middleware de fusion des documents Swagger des services aval.</summary>
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

                var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                var cacheKey = "swagger_merged";

                if (cache.TryGetValue(cacheKey, out string? cached) && cached != null)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(cached);
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
                    var log = context.RequestServices.GetRequiredService<ILogger<HttpContext>>();
                    log.LogWarning("Swagger aggregation failed: all downstream documents unavailable");
                    context.Response.StatusCode = 503;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($"{{\"message\":\"{Messages.Swagger.DownstreamUnavailable}\"}}");
                    return;
                }

                var merged = MergeSwaggerDocuments(validResults, context.Request.Scheme, context.Request.Host);
                cache.Set(cacheKey, merged, TimeSpan.FromHours(1));
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(merged);
            }
            catch (Exception ex)
            {
                var log = context.RequestServices.GetRequiredService<ILogger<HttpContext>>();
                log.LogError(ex, "Swagger merge middleware error");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"{{\"error\":\"{Messages.Swagger.MergeFailed}\"}}");
            }
        });

        return app;
    }

    /// <summary>Mappe les endpoints de santé du Gateway (/health, /health/downstream, /health/ready, /health/services, /metrics/latency).</summary>
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
                        var healthCheckService = context.RequestServices.GetRequiredService<HealthCheckService>();
                        var report = await healthCheckService.CheckHealthAsync();
                        var overallStatus = report.Status.ToString();

                        await response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            status = overallStatus,
                            service = "DotnetNiger.Gateway",
                            timestamp = DateTime.UtcNow,
                            checks = report.Entries.ToDictionary(e => e.Key, e => new
                            {
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                data = e.Value.Data?.Count > 0 ? e.Value.Data : null
                            })
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

    /// <summary>Mappe l'endpoint /admin/clear-swagger-cache pour vider le cache Swagger manuellement.</summary>
    public static IApplicationBuilder MapCacheBusterEndpoint(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals("/admin/clear-swagger-cache", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 405;
                    return;
                }

                var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                cache.Remove("swagger_merged");
                var log = context.RequestServices.GetRequiredService<ILogger<HttpContext>>();
                log.LogInformation("Swagger cache cleared by admin request");

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { success = true, message = Messages.Swagger.CacheCleared }));
                return;
            }

            await next();
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
        catch (Exception)
        {
            // Swagger fetch failures are non-critical - logged at debug level
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

            var existingPaths = merged["paths"]?.AsObject();
            if (existingPaths == null)
            {
                existingPaths = [];
                merged["paths"] = existingPaths;
            }
            if (doc["paths"] is JsonObject docPaths)
                foreach (var p in docPaths)
                    existingPaths[p.Key] = p.Value?.DeepClone();

            var existingSchemas = merged["components"]?["schemas"]?.AsObject();
            if (existingSchemas == null)
            {
                existingSchemas = [];
                merged["components"] = new JsonObject { ["schemas"] = existingSchemas };
            }
            if (doc["components"]?["schemas"] is JsonObject docSchemas)
                foreach (var s in docSchemas)
                    if (!existingSchemas.ContainsKey(s.Key))
                        existingSchemas[s.Key] = s.Value?.DeepClone();
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
