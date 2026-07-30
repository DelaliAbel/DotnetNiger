using DotnetNiger.UI.Services.Contracts;
using Microsoft.JSInterop;

namespace DotnetNiger.UI.Services.App;

public class ConfirmService : IConfirmService
{
    private readonly IJSRuntime _js;

    public ConfirmService(IJSRuntime js) => _js = js;

    public async Task<bool> ShowAsync(string message)
    {
        try
        {
            return await _js.InvokeAsync<bool>("confirm", message);
        }
        catch
        {
            return false;
        }
    }
}
