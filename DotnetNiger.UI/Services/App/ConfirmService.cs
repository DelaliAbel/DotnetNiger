using DotnetNiger.UI.Services.Contracts;
using Microsoft.JSInterop;
using System.Threading;

namespace DotnetNiger.UI.Services.App;

public class ConfirmService : IConfirmService
{
    private readonly IJSRuntime _js;
    public event EventHandler<ConfirmRequest>? OnConfirm;

    public ConfirmService(IJSRuntime js) => _js = js;

    public Task<bool> ShowAsync(string message, CancellationToken cancellationToken = default)
    {
        var request = new ConfirmRequest
        {
            Message = message,
            CompletionSource = new TaskCompletionSource<bool>()
        };

        OnConfirm?.Invoke(this, request);

        return request.CompletionSource.Task;
    }
}
