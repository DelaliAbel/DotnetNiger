using System.Threading;
namespace DotnetNiger.UI.Services.Contracts;

public class ConfirmRequest
{
    public string Message { get; set; } = string.Empty;
    public TaskCompletionSource<bool> CompletionSource { get; set; } = new();
}

public interface IConfirmService
{
    Task<bool> ShowAsync(string message, CancellationToken cancellationToken = default);
    event EventHandler<ConfirmRequest>? OnConfirm;
}
