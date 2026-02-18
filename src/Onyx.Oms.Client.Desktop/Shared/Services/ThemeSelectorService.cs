using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    public ElementTheme Theme { get; private set; } = ElementTheme.Default;

    private readonly ISettingsService _settingsService;

    public ThemeSelectorService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;
            
            // Optional: Update TitleBar if custom
            // TitleBarHelper.UpdateTitleBar(Theme);
        }

        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        Theme = LoadThemeFromSettings();
        await SetRequestedThemeAsync();
    }

    private ElementTheme LoadThemeFromSettings()
    {
        var themeName = _settingsService.GetValue<string>(SettingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        _settingsService.SetValue(SettingsKey, theme.ToString());
        await Task.CompletedTask;
    }
}
