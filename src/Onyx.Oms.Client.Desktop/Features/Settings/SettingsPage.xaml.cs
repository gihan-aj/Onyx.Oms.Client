using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Settings;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.Current.Services.GetService(typeof(SettingsViewModel)) as SettingsViewModel
            ?? throw new InvalidOperationException("SettingsViewModel not found");

        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // sync theme selection
        var currentTheme = ViewModel.CurrentTheme;
        foreach (ComboBoxItem item in ThemeSelector.Items)
        {
            if (item.Tag is string tag && System.Enum.TryParse<ElementTheme>(tag, out var theme))
            {
                if (theme == currentTheme)
                {
                    ThemeSelector.SelectedItem = item;
                    break;
                }
            }
        }
    }

    private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item && item.Tag is string themeTag)
        {
            if (System.Enum.TryParse<ElementTheme>(themeTag, out var theme))
            {
                ViewModel.SwitchThemeCommand.Execute(theme);
            }
        }
    }
}
