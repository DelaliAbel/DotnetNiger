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

        var wrapped = Deserialize<ApiSuccessResponse<T>>(json);
        if (wrapped is not null)
            return wrapped.Data;

        return Deserialize<T>(json);
    }

    public static async Task<List<T>> ReadCollectionAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();

        var wrappedList = Deserialize<ApiSuccessResponse<List<T>>>(json);
        if (wrappedList?.Data is not null)
            return wrappedList.Data;

        var wrappedPaginated = Deserialize<ApiSuccessResponse<PaginatedDto<T>>>(json);
        if (wrappedPaginated?.Data?.Items is not null)
            return wrappedPaginated.Data.Items;

        var directList = Deserialize<List<T>>(json);
        if (directList is not null)
            return directList;

        var directPaginated = Deserialize<PaginatedDto<T>>(json);
        return directPaginated?.Items ?? new List<T>();
    }

    private static T? Deserialize<T>(string json)
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
