using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class LicenseValidationView : UserControl
{
    private readonly ILicenseManagerService _licenseManager;
    
    public event EventHandler? LicenseValidated;

    // We can inject MainWindow reference from MainWindow.xaml.cs so FilePicker has an HWND
    public Window? ParentWindow { get; set; }

    public LicenseValidationView()
    {
        InitializeComponent();
        _licenseManager = App.Current.Services.GetRequiredService<ILicenseManagerService>();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        HardwareIdBox.Text = _licenseManager.GetHardwareId();
    }

    private void CopyHardwareId_Click(object sender, RoutedEventArgs e)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(HardwareIdBox.Text);
        Clipboard.SetContent(dataPackage);
    }

    private async void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorBanner.IsOpen = false;
        
        if (ParentWindow == null)
            ParentWindow = App.MainWindow;

        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        
        // Initialize the FileOpenPicker with the Window Handle (HWND)
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(ParentWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.FileTypeFilter.Add(".key");

        UploadButton.Visibility = Visibility.Collapsed;
        LoadingSpinner.Visibility = Visibility.Visible;
        LoadingSpinner.IsActive = true;

        var file = await picker.PickSingleFileAsync();
        
        if (file != null)
        {
            try
            {
                // Read and validate first before overwriting
                string keyContents = await FileIO.ReadTextAsync(file);
                
                if (_licenseManager.IsKeyValid(keyContents))
                {
                    // Copy to LocalFolder as license.key
                    var localFolder = ApplicationData.Current.LocalFolder;
                    await file.CopyAsync(localFolder, "license.key", NameCollisionOption.ReplaceExisting);

                    SuccessBanner.IsOpen = true;
                    
                    // Delay slightly so user sees success message
                    await Task.Delay(1500);
                    
                    LicenseValidated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorBanner.IsOpen = true;
                }
            }
            catch (Exception)
            {
                ErrorBanner.Message = "Error reading or saving the license file.";
                ErrorBanner.IsOpen = true;
            }
        }
        
        LoadingSpinner.IsActive = false;
        LoadingSpinner.Visibility = Visibility.Collapsed;
        UploadButton.Visibility = Visibility.Visible;
    }
}
