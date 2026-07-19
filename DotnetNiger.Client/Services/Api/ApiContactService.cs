using System.Net.Http.Json;
using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Api;

public class ApiContactService : ApiServiceBase, IContactService
{
    public ApiContactService(HttpClient http) : base(http)
    {
    }

    public async Task<bool> SendAsync(ContactRequest request)
    {
        var response = await Http.PostAsJsonAsync(ApiEndpoints.Contact, request);
        return response.IsSuccessStatusCode;
    }
}
