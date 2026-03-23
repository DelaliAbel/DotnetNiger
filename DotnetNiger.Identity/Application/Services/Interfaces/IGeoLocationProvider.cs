namespace DotnetNiger.Identity.Application.Services.Interfaces;

/// <summary>
/// Represents geolocation data for an IP address
/// </summary>
public class GeoLocationData
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}

/// <summary>
/// Service for resolving IP address geolocation
/// </summary>
public interface IGeoLocationProvider
{
    /// <summary>
    /// Gets geolocation data for the provided IP address
    /// </summary>
    /// <param name="ipAddress">IP address to geolocate</param>
    /// <returns>Geolocation data or empty data if unable to determine</returns>
    Task<GeoLocationData> GetAsync(string ipAddress, CancellationToken cancellationToken = default);
}
