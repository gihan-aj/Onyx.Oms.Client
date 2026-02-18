using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Onyx.Oms.Client.Desktop.Features.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeSelectorService _themeSelectorService;

    private ElementTheme _currentTheme;
    public ElementTheme CurrentTheme
    {
        get => _currentTheme;
        set => SetProperty(ref _currentTheme, value);
    }

    private string _versionDescription;
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _currentTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
    }

    [RelayCommand]
    private async Task SwitchTheme(ElementTheme theme)
    {
        if (CurrentTheme != theme)
        {
            CurrentTheme = theme;
            await _themeSelectorService.SetThemeAsync(theme);
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}

// Helper for versions
internal static class RuntimeHelper
{
    public static bool IsMSIX
    {
        get
        {
            var length = 0;
            return GetCurrentPackageFullName(ref length, null) != 15700L;
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, System.Text.StringBuilder? packageFullName);
}

internal static class StringExtensions
{
    public static string GetLocalized(this string resourceKey)
    {
        // Simple fallback for now
        return "Onyx OMS"; 
    }
}
