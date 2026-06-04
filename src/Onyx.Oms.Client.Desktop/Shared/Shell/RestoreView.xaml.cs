using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class RestoreView : UserControl
{
    public BackgroundProcessService? BackgroundProcessService { get; set; }

    public DatabaseRestoreService? DatabaseRestoreService { get; set; }

    public Window? ParentWindow { get; set; }

    public event EventHandler? RestoreCanceled;

    public event EventHandler? RestoreCompleted;

    private string _idpPath = string.Empty;

    private string _omsPath = string.Empty;

    public RestoreView()
    {
        InitializeComponent();
    }

    private async void BrowseIdp_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickBackupFileAsync("Select Identity Provider Backup (OnyxIdP)");
        if (path is null) return;
        _idpPath = path;
        IdpPathBox.Text = path;
        UpdateRestoreButton();
    }
    private async void BrowseOms_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickBackupFileAsync("Select OMS Backup (OnyxOms)");
        if (path is null) return;
        _omsPath = path;
        OmsPathBox.Text = path;
        UpdateRestoreButton();
    }

    private void UpdateRestoreButton()
    {
        RestoreBtn.IsEnabled = !string.IsNullOrEmpty(_idpPath) && !string.IsNullOrEmpty(_omsPath);
    }

    private async Task<string?> PickBackupFileAsync(string commitButtonText)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            CommitButtonText = commitButtonText
        };
        picker.FileTypeFilter.Add(".bak");
        // WinUI 3 requires attaching the picker to the window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(ParentWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        RestoreCanceled?.Invoke(this, EventArgs.Empty);
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        ErrorBar.IsOpen = false;
        RestoreBtn.IsEnabled = false;
        // Show blocking progress dialog
        var dialog = new ContentDialog
        {
            Title = "Restoring System...",
            Content = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new ProgressRing { IsActive = true, HorizontalAlignment = HorizontalAlignment.Center },
                    new TextBlock
                    {
                        Text = "Overwriting databases with backup data. Please do not close the application...",
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 320,
                        TextAlignment = TextAlignment.Center
                    }
                },
                HorizontalAlignment = HorizontalAlignment.Center
            },
            XamlRoot = this.XamlRoot,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false
        };
        _ = dialog.ShowAsync();
        try
        {
            await BackgroundProcessService!.StopBackendServicesAsync();
            bool success = await DatabaseRestoreService!.RestoreSystemAsync(_idpPath, _omsPath);
            BackgroundProcessService!.ResetReadyState();
            BackgroundProcessService.StartBackendServices();
            if (success)
            {
                await BackgroundProcessService.WaitForApiToWakeUpAsync();
                dialog.Hide();
                RestoreCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                dialog.Hide();
                ErrorBar.Message = "Restore failed. Please verify the backup files and try again.";
                ErrorBar.IsOpen = true;
                RestoreBtn.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            BackgroundProcessService?.StartBackendServices();
            dialog.Hide();
            ErrorBar.Message = $"Unexpected error: {ex.Message}";
            ErrorBar.IsOpen = true;
            RestoreBtn.IsEnabled = true;
        }
    }

    private void Reset()
    {
        _idpPath = string.Empty;
        _omsPath = string.Empty;
        IdpPathBox.Text = string.Empty;
        OmsPathBox.Text = string.Empty;
        RestoreBtn.IsEnabled = false;
        ErrorBar.IsOpen = false;
    }
}
