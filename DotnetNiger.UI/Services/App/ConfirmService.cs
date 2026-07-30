using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.App;

public class ConfirmService : IConfirmService
{
    public event EventHandler<ConfirmRequest>? OnConfirm;

    public Task<bool> ShowAsync(string message)
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
