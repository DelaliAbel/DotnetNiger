namespace DotnetNiger.UI.Services.Contracts;

public interface IConfirmService
{
    Task<bool> ShowAsync(string message);
}
