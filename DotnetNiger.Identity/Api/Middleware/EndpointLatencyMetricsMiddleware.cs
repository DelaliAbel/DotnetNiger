using System.Collections.Concurrent;
using System.Diagnostics;

namespace DotnetNiger.Identity.Api.Middleware;

public sealed class EndpointLatencyMetricsMiddleware
{
    private const int MaxSamples = 2048;
    private static readonly ConcurrentDictionary<string, EndpointLatencyWindow> Windows = new(StringComparer.OrdinalIgnoreCase);
    private readonly RequestDelegate _next;

    public EndpointLatencyMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/metrics/latency", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var route = context.GetEndpoint()?.DisplayName ?? path;
        var key = $"{context.Request.Method} {route}";

        var window = Windows.GetOrAdd(key, _ => new EndpointLatencyWindow(MaxSamples));
        window.Record(stopwatch.Elapsed.TotalMilliseconds, context.Response.StatusCode);
    }

    public static object GetSnapshot(int top = 5)
    {
        var endpoints = Windows
            .Select(item => new
            {
                endpoint = item.Key,
                metrics = item.Value.Snapshot()
            })
            .Where(item => item.metrics.Count > 0)
            .OrderByDescending(item => item.metrics.P95Ms)
            .Take(Math.Max(1, top))
            .ToList();

        return new
        {
            generatedAtUtc = DateTime.UtcNow,
            endpointCount = Windows.Count,
            topSlowest = endpoints
        };
    }

    private sealed class EndpointLatencyWindow
    {
        private readonly int _maxSamples;
        private readonly object _lock = new();
        private readonly Queue<double> _latencies = new();
        private long _requestCount;
        private long _errorCount;

        public EndpointLatencyWindow(int maxSamples)
        {
            _maxSamples = maxSamples;
        }

        public void Record(double elapsedMs, int statusCode)
        {
            lock (_lock)
            {
                _requestCount++;
                if (statusCode >= 500)
                {
                    _errorCount++;
                }

                _latencies.Enqueue(elapsedMs);
                while (_latencies.Count > _maxSamples)
                {
                    _latencies.Dequeue();
                }
            }
        }

        public EndpointSnapshot Snapshot()
        {
            lock (_lock)
            {
                if (_latencies.Count == 0)
                {
                    return EndpointSnapshot.Empty;
                }

                var values = _latencies.OrderBy(value => value).ToArray();
                return new EndpointSnapshot
                {
                    Count = values.Length,
                    TotalRequests = _requestCount,
                    TotalErrors = _errorCount,
                    P50Ms = Percentile(values, 0.50),
                    P95Ms = Percentile(values, 0.95),
                    P99Ms = Percentile(values, 0.99)
                };
            }
        }

        private static double Percentile(double[] sortedValues, double percentile)
        {
            if (sortedValues.Length == 0)
            {
                return 0;
            }

            var index = (int)Math.Ceiling(percentile * sortedValues.Length) - 1;
            index = Math.Clamp(index, 0, sortedValues.Length - 1);
            return Math.Round(sortedValues[index], 2);
        }
    }

    private sealed class EndpointSnapshot
    {
        public static EndpointSnapshot Empty { get; } = new();

        public int Count { get; init; }
        public long TotalRequests { get; init; }
        public long TotalErrors { get; init; }
        public double P50Ms { get; init; }
        public double P95Ms { get; init; }
        public double P99Ms { get; init; }
    }
}
