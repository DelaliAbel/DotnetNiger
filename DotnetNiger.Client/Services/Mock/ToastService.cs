using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Mock;

public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnShow;

    public void ShowToast(string message, ToastLevel level = ToastLevel.Info)
    {
        OnShow?.Invoke(new ToastMessage
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.UtcNow
        });
    }
}
