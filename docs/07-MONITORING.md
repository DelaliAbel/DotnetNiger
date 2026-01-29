# 📊 Monitoring & Observabilité

Guide de monitoring, logging et observabilité.

## Vue d'ensemble

DotnetNiger utilise une stack complète de monitoring:

- **Serilog** - Structured logging
- **Application Insights** - APM Azure
- **Prometheus** - Métriques
- **Grafana** - Dashboards
- **Health Checks** - Santé des services

## Logging avec Serilog

### Configuration

**appsettings.json:**

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/dotnetniger-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Utilisation

```csharp
public class PostService
{
    private readonly ILogger<PostService> _logger;

    public PostService(ILogger<PostService> logger)
    {
        _logger = logger;
    }

    public async Task<Post> CreateAsync(CreatePostDto dto)
    {
        _logger.LogInformation("Creating post for user {UserId}", dto.UserId);

        try
        {
            var post = await _repository.AddAsync(new Post { ... });
            _logger.LogInformation("Post {PostId} created successfully", post.Id);
            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post for user {UserId}", dto.UserId);
            throw;
        }
    }
}
```

## Application Insights

### Configuration

```csharp
services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

### Custom Metrics

```csharp
_telemetryClient.TrackMetric("PostsCreated", 1);
_telemetryClient.TrackEvent("UserRegistered",
    new Dictionary<string, string>
    {
        { "UserId", userId.ToString() },
        { "Source", "Web" }
    });
```

### Dependencies

```csharp
using (_telemetryClient.StartOperation<DependencyTelemetry>("SQL Query"))
{
    var result = await _repository.GetAsync(id);
}
```

## Prometheus Metrics

### Configuration

```csharp
services.AddMetrics();
services.UseHttpMetrics();

app.MapMetrics(); // Expose /metrics endpoint
```

### Custom Metrics

```csharp
public class MetricsService
{
    private static readonly Counter _requestsTotal =
        Metrics.CreateCounter("http_requests_total", "Total HTTP requests");

    private static readonly Histogram _requestDuration =
        Metrics.CreateHistogram("http_request_duration_seconds",
            "HTTP request duration");

    private static readonly Gauge _activeConnections =
        Metrics.CreateGauge("active_connections", "Active connections");

    public void RecordRequest()
    {
        _requestsTotal.Inc();
    }

    public void RecordDuration(double seconds)
    {
        _requestDuration.Observe(seconds);
    }
}
```

## Health Checks

### Configuration

```csharp
services.AddHealthChecks()
    .AddSqlServer(
        connectionString: configuration.GetConnectionString("Identity"),
        name: "identity-db",
        tags: new[] { "db", "sql" })
    .AddRedis(
        redisConnectionString: configuration["Redis:ConnectionString"],
        name: "redis-cache",
        tags: new[] { "cache", "redis" })
    .AddUrlGroup(
        uri: new Uri("http://localhost:5075/health"),
        name: "identity-service",
        tags: new[] { "service" })
    .AddCheck<CustomHealthCheck>("custom-check");
```

### Custom Health Check

```csharp
public class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var isHealthy = CheckSomething();

        if (isHealthy)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("All systems operational"));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy("System is degraded"));
    }
}
```

### Endpoints

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

## Grafana Dashboards

### Datasources

1. **Prometheus** - Métriques applicatives
2. **Loki** - Logs centralisés
3. **Application Insights** - APM

### Dashboards Recommandés

**System Metrics:**

- CPU Usage
- Memory Usage
- Disk I/O
- Network Traffic

**Application Metrics:**

- Request Rate
- Error Rate
- Response Time (p50, p95, p99)
- Active Connections

**Business Metrics:**

- New Users/Day
- Posts Created/Hour
- Comments/Minute
- Active Users

## Distributed Tracing

### Configuration

```csharp
services.AddOpenTelemetryTracing(builder =>
{
    builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        });
});
```

### Correlation ID

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[CorrelationIdHeader] = correlationId;
        context.Response.Headers.Add(CorrelationIdHeader, correlationId);

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
```

## Alerting

### Application Insights Alerts

```json
{
  "name": "High Error Rate",
  "condition": {
    "allOf": [
      {
        "metricName": "requests/failed",
        "operator": "GreaterThan",
        "threshold": 10,
        "timeAggregation": "Average",
        "windowSize": "PT5M"
      }
    ]
  },
  "actions": [
    {
      "actionGroupId": "/subscriptions/.../actionGroups/devops-team",
      "webhookProperties": {}
    }
  ]
}
```

### Prometheus Alertmanager

```yaml
groups:
  - name: dotnetniger_alerts
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value }} errors/sec"

      - alert: HighResponseTime
        expr: http_request_duration_seconds{quantile="0.99"} > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "99th percentile is {{ $value }}s"
```

## Performance Monitoring

### Key Metrics

| Metric              | Target      | Critical   |
| ------------------- | ----------- | ---------- |
| Response Time (p95) | < 200ms     | > 1s       |
| Error Rate          | < 0.1%      | > 1%       |
| Throughput          | > 100 req/s | < 10 req/s |
| CPU Usage           | < 70%       | > 90%      |
| Memory Usage        | < 80%       | > 95%      |

### Monitoring Checklist

- [ ] Logs centralisés (Seq/Loki)
- [ ] Métriques exposées (Prometheus)
- [ ] Health checks configurés
- [ ] Dashboards Grafana créés
- [ ] Alertes configurées
- [ ] Tracing distribué activé
- [ ] APM configuré (App Insights)

---

**Dernière mise à jour:** 29 Janvier 2026
