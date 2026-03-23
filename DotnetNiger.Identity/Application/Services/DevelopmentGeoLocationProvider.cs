using DotnetNiger.Identity.Application.Services.Interfaces;

namespace DotnetNiger.Identity.Application.Services;

/// <summary>
/// Development/Mock implementation of geolocation provider
/// Provides placeholder geolocation data for IP addresses
/// In production, this should be replaced with MaxMind GeoIP2, IPGeolocation.io, or similar service
/// </summary>
public class DevelopmentGeoLocationProvider : IGeoLocationProvider
{
    private static readonly Dictionary<string, GeoLocationData> KnownIPs = new()
    {
        // Localhost addresses
        {
            "127.0.0.1",
            new GeoLocationData
            {
                Country = "Local",
                City = "Localhost",
                Latitude = 0,
                Longitude = 0,
                TimeZone = "UTC"
            }
        },
        {
            "::1",
            new GeoLocationData
            {
                Country = "Local",
                City = "Localhost",
                Latitude = 0,
                Longitude = 0,
                TimeZone = "UTC"
            }
        }
    };

    public Task<GeoLocationData> GetAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return Task.FromResult(GetDefaultData());
        }

        // Check if we have data for this IP
        if (KnownIPs.TryGetValue(ipAddress, out var data))
        {
            return Task.FromResult(data);
        }

        // Return default data for unknown IPs
        // In production, make HTTP call to geolocation API here
        return Task.FromResult(GetDefaultData());
    }

    private static GeoLocationData GetDefaultData()
    {
        return new GeoLocationData
        {
            Country = "Unknown",
            City = "Unknown",
            Latitude = 0,
            Longitude = 0,
            TimeZone = "UTC"
        };
    }
}
