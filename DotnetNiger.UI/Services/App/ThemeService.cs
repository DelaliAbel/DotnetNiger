using Microsoft.JSInterop;
using System.Threading;

namespace DotnetNiger.UI.Services.App;

public class ThemeService
{
    private readonly IJSRuntime _js;
    private string _currentTheme = "light";

    public event Action? OnThemeChanged;

    public string CurrentTheme => _currentTheme;
    public bool IsDarkMode => _currentTheme == "dark";

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var theme = await _js.InvokeAsync<string?>("localStorage.getItem", "theme");
            _currentTheme = theme ?? "light";
            await ApplyThemeAsync(_currentTheme);
        }
        catch
        {
            _currentTheme = "light";
        }
    }

    public async Task ToggleThemeAsync(CancellationToken cancellationToken = default)
    {
        _currentTheme = _currentTheme == "light" ? "dark" : "light";
        await ApplyThemeAsync(_currentTheme);
        OnThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(string theme, CancellationToken cancellationToken = default)
    {
        _currentTheme = theme;
        await ApplyThemeAsync(theme);
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync(string theme)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "theme", theme);
        await _js.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", theme);
    }
}
