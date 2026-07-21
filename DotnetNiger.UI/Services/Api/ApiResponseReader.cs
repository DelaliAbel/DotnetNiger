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
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return default;

        var wrapped = TryDeserialize<ApiSuccessResponse<T>>(json);
        if (wrapped is not null)
            return wrapped.Data;

        return TryDeserialize<T>(json);
    }

    public static async Task<List<T>> ReadCollectionAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return new List<T>();

        var hasDataProperty = doc.RootElement.TryGetProperty("data", out var data);

        if (!hasDataProperty)
        {
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "data", StringComparison.OrdinalIgnoreCase))
                {
                    data = prop.Value;
                    hasDataProperty = true;
                    break;
                }
            }
        }

        var isPaginated = hasDataProperty && data.ValueKind == JsonValueKind.Object;

        if (isPaginated)
        {
            var paginated = TryDeserialize<ApiSuccessResponse<PaginatedDto<T>>>(json);
            if (paginated?.Data?.Items is not null)
                return paginated.Data.Items;

            var directPaginated = TryDeserialize<PaginatedDto<T>>(json);
            if (directPaginated?.Items is not null)
                return directPaginated.Items;
        }
        else
        {
            var list = TryDeserialize<ApiSuccessResponse<List<T>>>(json);
            if (list?.Data is not null)
                return list.Data;

            var directList = TryDeserialize<List<T>>(json);
            if (directList is not null)
                return directList;
        }

        return new List<T>();
    }

    private static T? TryDeserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch
        {
            return default;
        }
    }
}
