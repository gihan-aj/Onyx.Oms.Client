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

    private void OnStateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is string selectedState)
        {
            ViewModel.UpdateDistricts(selectedState);
        }
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Pass navigation event to ViewModel
        if (ViewModel is Onyx.Oms.Client.Desktop.Shared.Services.INavigationAware navAware)
        {
            navAware.OnNavigatedTo(e.Parameter);
        }

        // Set default selector bar item
        if (SettingsSelectorBar.Items.Count > 0)
        {
            SettingsSelectorBar.SelectedItem = SettingsSelectorBar.Items[0];
        }
        
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

    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel is Onyx.Oms.Client.Desktop.Shared.Services.INavigationAware navAware)
        {
            navAware.OnNavigatedFrom();
        }
    }

    private void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        if (selectedItem == null) return;

        string? tag = selectedItem.Tag.ToString();
        GeneralSettingsGrid.Visibility = tag == "General" ? Visibility.Visible : Visibility.Collapsed;
        RegionalSettingsGrid.Visibility = tag == "Regional" ? Visibility.Visible : Visibility.Collapsed;
        SequencesGrid.Visibility = tag == "Sequences" ? Visibility.Visible : Visibility.Collapsed;
        AppearanceGrid.Visibility = tag == "Appearance" ? Visibility.Visible : Visibility.Collapsed;
        AboutGrid.Visibility = tag == "About" ? Visibility.Visible : Visibility.Collapsed;
        WhatsAppSettingsGrid.Visibility = tag == "WhatsApp" ? Visibility.Visible : Visibility.Collapsed;
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

    private void OnWhatsAppAccessTokenChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            ViewModel.WhatsApp.AccessToken = passwordBox.Password;
        }
    }
}
