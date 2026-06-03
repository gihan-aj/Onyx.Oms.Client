using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

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
        DatabaseSettingsGrid.Visibility = tag == "Database" ? Visibility.Visible : Visibility.Collapsed;
        PaymentMethodsGrid.Visibility = tag == "PaymentMethods" ? Visibility.Visible : Visibility.Collapsed;
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

    private void OnEditBackupSettingsClick(object sender, RoutedEventArgs e)
    => ViewModel.Backup.BeginEdit();
    private void OnCancelBackupSettingsClick(object sender, RoutedEventArgs e)
        => ViewModel.Backup.CancelEdit();
    private void OnSaveBackupSettingsClick(object sender, RoutedEventArgs e)
        => ViewModel.Backup.Save();

    private async void OnBrowseBackupPathClick(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");   // required even for folders
                                          // WinUI 3 — attach window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ViewModel.Backup.BackupPath = folder.Path;
        }
    }

    private async void OnRestoreDatabaseClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Restore Database?",
            Content = "This will permanently overwrite all current data with the selected backup. Are you absolutely sure?",
            PrimaryButtonText = "Yes, Restore",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        // Style the primary button red to reinforce the danger
        dialog.PrimaryButtonStyle = (Style)Application.Current.Resources["DefaultButtonStyle"];
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // TODO: call ViewModel.Backup.RestoreAsync() once the endpoint is ready
            ViewModel.IsBusy = true; // placeholder — remove when wired
            await Task.Delay(500);
            ViewModel.IsBusy = false;
            var toast = new ContentDialog
            {
                Title = "Not yet available",
                Content = "The restore endpoint is not yet implemented. Please check back in a future release.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await toast.ShowAsync();
        }
    }
}
