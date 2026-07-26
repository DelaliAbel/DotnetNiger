using System.Net.Http.Json;
using System.Text.Json;
using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Api;

internal static class ApiResponseReader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        try
        {
            if (response.Content.Headers.ContentLength == 0)
                return default;

            var wrapped = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<T>>(Options);
            if (wrapped is not null && wrapped.Success)
                return wrapped.Data;

            return await response.Content.ReadFromJsonAsync<T>(Options);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ApiResponseReader] ReadAsync<{typeof(T).Name}> failed: {ex.Message}");
            return default;
        }
    }

    public static async Task<List<T>> ReadCollectionAsync<T>(HttpResponseMessage response)
    {
        try
        {
            if (response.Content.Headers.ContentLength == 0)
                return new List<T>();

            var doc = await response.Content.ReadFromJsonAsync<JsonElement>(Options);

            if (doc.ValueKind == JsonValueKind.Array)
            {
                return doc.Deserialize<List<T>>(Options) ?? new List<T>();
            }

            if (doc.ValueKind != JsonValueKind.Object)
                return new List<T>();

            JsonElement data = default;
            bool hasData = false;

            foreach (var prop in doc.EnumerateObject())
            {
                if (string.Equals(prop.Name, "data", StringComparison.OrdinalIgnoreCase))
                {
                    data = prop.Value;
                    hasData = true;
                    break;
                }
            }

            if (hasData)
            {
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    return items.Deserialize<List<T>>(Options) ?? new List<T>();
                }
                
                if (data.ValueKind == JsonValueKind.Array)
                {
                    return data.Deserialize<List<T>>(Options) ?? new List<T>();
                }
            }

            Console.Error.WriteLine($"[ApiResponseReader] ReadCollectionAsync<{typeof(T).Name}>: no 'data' property found in response");
            return new List<T>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ApiResponseReader] ReadCollectionAsync<{typeof(T).Name}> failed: {ex.Message}");
            return new List<T>();
        }
    }
}
