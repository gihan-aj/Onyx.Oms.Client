# Theming & Settings Guide

This guide explains how to implement a **Settings Page** and a persistent **Theming Service**, inspired by the WinUI Gallery but simplified for your Order Management System.

## 1. Concepts
*   **ThemeHelper**: A static class (or service) that manages the current `ElementTheme` (Light/Dark/System) and applies it to the `Window` and `TitleBar`.
*   **SettingsService**: A service to persist user preferences. The Gallery uses a complex "Provider" pattern, but for most apps, wrapping `ApplicationData.Current.LocalSettings` is sufficient.
*   **SettingsPage**: The UI to toggle these settings.

## 2. Settings Service (Persistence)
Create `Services/LocallSettingsService.cs`. This handles saving/loading data.

```csharp
using Windows.Storage;

public static class LocalSettingsService
{
    private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

    public static T ReadSetting<T>(string key, T defaultValue)
    {
        if (LocalSettings.Values.TryGetValue(key, out var obj))
        {
            return (T)obj;
        }
        return defaultValue;
    }

    public static void SaveSetting<T>(string key, T value)
    {
        LocalSettings.Values[key] = value;
    }
}
```
*(Note: If you are building an unpackaged app, `ApplicationData` won't work easily. You might need a JSON-based file approach, but standard WinUI 3 (Packaged) uses the above.)*

## 3. Theme Service
Update your `ThemeHelper.cs` (from `docs/services.md`) to use `LocalSettingsService`.

```csharp
// Services/ThemeHelper.cs
using Microsoft.UI.Xaml;

public static class ThemeHelper
{
    private const string SelectedAppThemeKey = "SelectedAppTheme";

    public static ElementTheme RootTheme
    {
        get
        {
            // Load saved theme (Default if none saved)
            var savedTheme = LocalSettingsService.ReadSetting(SelectedAppThemeKey, (int)ElementTheme.Default);
            return (ElementTheme)savedTheme;
        }
        set
        {
            // Apply to Window
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = value;
            }

            // Save to Settings
            LocalSettingsService.SaveSetting(SelectedAppThemeKey, (int)value);
            
            // Update TitleBar buttons
            TitleBarHelper.ApplySystemThemeToCaptionButtons(App.MainWindow, value);
        }
    }

    public static void Initialize()
    {
        // Call this in App.xaml.cs OnLaunched
        RootTheme = RootTheme; // Trigger setter to apply saved theme
    }
}
```

## 4. Settings Page UI
Create `Views/SettingsPage.xaml`. Use `RadioButtons` or `ComboBox` for theme selection.

**Prerequisite**: Ensure you have the **Community Toolkit** installed for `SettingsCard` controls (as seen in Gallery), OR use standard `StackPanel` + `TextBlock` + `ComboBox`. The example below uses standard controls for simplicity.

```xml
<Page x:Class="MyOrderApp.Views.SettingsPage" ...>
    <StackPanel Spacing="20" Padding="24">
        <TextBlock Text="Settings" Style="{StaticResource TitleTextBlockStyle}" />

        <!-- Theme Setting -->
        <StackPanel Spacing="8">
            <TextBlock Text="App Theme" Style="{StaticResource SubtitleTextBlockStyle}" />
            <ComboBox x:Name="ThemeSelector" SelectionChanged="ThemeSelector_SelectionChanged">
                <ComboBoxItem Content="Light" Tag="Light" />
                <ComboBoxItem Content="Dark" Tag="Dark" />
                <ComboBoxItem Content="Use System Setting" Tag="Default" />
            </ComboBox>
        </StackPanel>
        
        <!-- About Section -->
        <TextBlock Text="About" Style="{StaticResource SubtitleTextBlockStyle}" Margin="0,20,0,0"/>
        <TextBlock Text="Order Management System v1.0" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
    </StackPanel>
</Page>
```

**Code Behind (`SettingsPage.xaml.cs`):**

```csharp
public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Set current selection based on ThemeHelper
        var currentTheme = ThemeHelper.RootTheme;
        ThemeSelector.SelectedIndex = (int)currentTheme; 
        // Note: Enum values: Default=0, Light=1, Dark=2. 
        // Adjust index logic if your ComboBox items order matches the Enum.
        // Better implementation: use Tag.
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeSelector.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            if (Enum.TryParse<ElementTheme>(tag, out var newTheme))
            {
                ThemeHelper.RootTheme = newTheme;
            }
        }
    }
}
```

## 5. Saving Custom Settings
To save other settings (e.g., "Default Warehouse ID"), just use the `LocalSettingsService`:

```csharp
// Save
LocalSettingsService.SaveSetting("DefaultWarehouseId", 101);

// Load
int warehouseId = LocalSettingsService.ReadSetting("DefaultWarehouseId", 0);
```
