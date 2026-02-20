namespace DotnetNiger.Gateway.Infrastructure.HttpClients;

/// <summary>
/// Classe de base pour les clients API
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly IConfiguration Configuration;
    protected readonly ILogger Logger;

    protected ApiClientBase(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger logger)
    {
        HttpClient = httpClient;
        Configuration = configuration;
        Logger = logger;
    }

    /// <summary>
    /// Récupère l'adresse de base d'un cluster
    /// </summary>
    protected string GetClusterAddress(string clusterName)
    {
        var address = Configuration.GetSection($"ReverseProxy:Clusters:{clusterName}:Destinations:destination1:Address").Value;
        if (string.IsNullOrEmpty(address))
        {
            Logger.LogWarning($"Cluster {clusterName} non trouvé dans la configuration");
            return string.Empty;
        }
        return address.TrimEnd('/');
    }

    /// <summary>
    /// Effectue une requête GET
    /// </summary>
    protected async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            Logger.LogInformation($"GET request à {url}");
            var response = await HttpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning($"Requête GET échouée: {response.StatusCode}");
                return default;
            }

            var content = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Erreur lors de la requête GET à {url}");
            return default;
        }
    }

    /// <summary>
    /// Effectue une requête POST
    /// </summary>
    protected async Task<T?> PostAsync<T>(string url, object? data = null)
    {
        try
        {
            Logger.LogInformation($"POST request à {url}");
            
            var content = data != null
                ? new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(data),
                    System.Text.Encoding.UTF8,
                    "application/json")
                : null;

            var response = await HttpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning($"Requête POST échouée: {response.StatusCode}");
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Erreur lors de la requête POST à {url}");
            return default;
        }
    }

    /// <summary>
    /// Effectue une requête PUT
    /// </summary>
    protected async Task<T?> PutAsync<T>(string url, object? data = null)
    {
        try
        {
            Logger.LogInformation($"PUT request à {url}");

            var content = data != null
                ? new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(data),
                    System.Text.Encoding.UTF8,
                    "application/json")
                : null;

            var response = await HttpClient.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning($"Requête PUT échouée: {response.StatusCode}");
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Erreur lors de la requête PUT à {url}");
            return default;
        }
    }

    /// <summary>
    /// Effectue une requête DELETE
    /// </summary>
    protected async Task<bool> DeleteAsync(string url)
    {
        try
        {
            Logger.LogInformation($"DELETE request à {url}");
            var response = await HttpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning($"Requête DELETE échouée: {response.StatusCode}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Erreur lors de la requête DELETE à {url}");
            return false;
        }
    }
}
