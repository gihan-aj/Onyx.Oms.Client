using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Onyx.Oms.Client.Desktop.Features.Settings;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ISettingsApi _settingsApi;
    private readonly IToastService _toastService;
    private readonly IPermissionService _permissionService;
    private readonly IFileService _fileService;
    private readonly ILogger<SettingsViewModel> _logger;

    private string _originalStoreInfoJson = string.Empty;
    private string _originalStoreAddressJson = string.Empty;
    private string _originalRegionalSettingsJson = string.Empty;

    public bool CanEditTenantSettings => _permissionService.CanExecute(Onyx.Oms.Client.Desktop.Shared.Constants.Permissions.Tenants.Edit);
    public bool CanEditAppSequences => _permissionService.CanExecute(Onyx.Oms.Client.Desktop.Shared.Constants.Permissions.AppSequences.Edit);

    private bool _isEditingStoreInfo;
    public bool IsEditingStoreInfo
    {
        get => _isEditingStoreInfo;
        set
        {
            if (SetProperty(ref _isEditingStoreInfo, value))
            {
                OnPropertyChanged(nameof(StoreInfoEditButtonVisible));
                OnPropertyChanged(nameof(StoreInfoSaveCancelVisible));
            }
        }
    }
    public Visibility StoreInfoEditButtonVisible => (CanEditTenantSettings && !IsEditingStoreInfo) ? Visibility.Visible : Visibility.Collapsed;
    public Visibility StoreInfoSaveCancelVisible => IsEditingStoreInfo ? Visibility.Visible : Visibility.Collapsed;

    //private string _district = string.Empty;
    //public string District { get => _district; set => SetProperty(ref _district, value); }

    private string[] _districts = Array.Empty<string>();
    public string[] Districts
    {
        get => _districts;
        private set => SetProperty(ref _districts, value);
    }

    public IReadOnlyList<string> Provinces { get; } = new[]
    {
        "Central", "Eastern", "North Central", "Northern", "North Western", "Sabaragamuwa", "Southern", "Uva", "Western"
    };

    private readonly Dictionary<string, string[]> _districtsByProvince = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Central", new[] { "Kandy", "Matale", "Nuwara Eliya" } },
        { "Eastern", new[] { "Ampara", "Batticaloa", "Trincomalee" } },
        { "North Central", new[] { "Anuradhapura", "Polonnaruwa" } },
        { "Northern", new[] { "Jaffna", "Kilinochchi", "Mannar", "Mullaitivu", "Vavuniya" } },
        { "North Western", new[] { "Kurunegala", "Puttalam" } },
        { "Sabaragamuwa", new[] { "Kegalle", "Ratnapura" } },
        { "Southern", new[] { "Galle", "Hambantota", "Matara" } },
        { "Uva", new[] { "Badulla", "Monaragala" } },
        { "Western", new[] { "Colombo", "Gampaha", "Kalutara" } }
    };

    public void UpdateDistricts(string province)
    {
        if (string.IsNullOrWhiteSpace(province) || !_districtsByProvince.TryGetValue(province, out var districts))
        {
            Districts = Array.Empty<string>();
        }
        else
        {
            Districts = districts;
        }

        if (!string.IsNullOrWhiteSpace(Profile?.StoreAddress?.District) && Array.IndexOf(Districts, Profile.StoreAddress.District) == -1)
        {
            Profile.StoreAddress.District = string.Empty;
        }
    }

    private bool _isEditingStoreAddress;
    public bool IsEditingStoreAddress
    {
        get => _isEditingStoreAddress;
        set
        {
            if (SetProperty(ref _isEditingStoreAddress, value))
            {
                OnPropertyChanged(nameof(StoreAddressEditButtonVisible));
                OnPropertyChanged(nameof(StoreAddressSaveCancelVisible));
            }
        }
    }
    public Visibility StoreAddressEditButtonVisible => (CanEditTenantSettings && !IsEditingStoreAddress) ? Visibility.Visible : Visibility.Collapsed;
    public Visibility StoreAddressSaveCancelVisible => IsEditingStoreAddress ? Visibility.Visible : Visibility.Collapsed;

    private BitmapImage? _logoImageSource;
    public BitmapImage? LogoImageSource
    {
        get => _logoImageSource;
        set
        {
            if (SetProperty(ref _logoImageSource, value))
            {
                OnPropertyChanged(nameof(HasLogo));
            }
        }
    }
    public bool HasLogo => LogoImageSource != null;

    private bool _isEditingRegionalSettings;
    public bool IsEditingRegionalSettings
    {
        get => _isEditingRegionalSettings;
        set
        {
            if (SetProperty(ref _isEditingRegionalSettings, value))
            {
                OnPropertyChanged(nameof(RegionalSettingsEditButtonVisible));
                OnPropertyChanged(nameof(RegionalSettingsSaveCancelVisible));
            }
        }
    }
    public Visibility RegionalSettingsEditButtonVisible => (CanEditTenantSettings && !IsEditingRegionalSettings) ? Visibility.Visible : Visibility.Collapsed;
    public Visibility RegionalSettingsSaveCancelVisible => IsEditingRegionalSettings ? Visibility.Visible : Visibility.Collapsed;

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

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private TenantProfileResponse _profile = new TenantProfileResponse { StoreAddress = new AddressDto() };
    public TenantProfileResponse Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    public ObservableCollection<AppSequenceViewModel> Sequences { get; } = new();

    public WhatsAppSettingsViewModel WhatsApp { get; }

    public PaymentMethodsViewModel PaymentMethods { get; }

    public IAsyncRelayCommand UploadLogoCommand { get; }
    public IAsyncRelayCommand DeleteLogoCommand { get; }

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        ISettingsApi settingsApi,
        IToastService toastService,
        IPermissionService permissionService,
        ILogger<SettingsViewModel> logger,
        IFileService fileService)
    {
        _themeSelectorService = themeSelectorService;
        _settingsApi = settingsApi;
        _toastService = toastService;
        _permissionService = permissionService;
        _fileService = fileService;
        _logger = logger;

        _currentTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        WhatsApp = new WhatsAppSettingsViewModel(settingsApi, toastService);
        PaymentMethods = new PaymentMethodsViewModel(settingsApi, permissionService);

        UploadLogoCommand = new AsyncRelayCommand(UploadLogoAsync);
        DeleteLogoCommand = new AsyncRelayCommand(DeleteLogoAsync);
    }

    public void OnNavigatedTo(object parameter)
    {
        _ = LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            Profile = await _settingsApi.GetTenantProfile();
            
            if (Profile.StoreAddress == null)
            {
                Profile.StoreAddress = new AddressDto();
            }

            IsEditingStoreInfo = false;
            IsEditingStoreAddress = false;
            IsEditingRegionalSettings = false;

            _originalStoreInfoJson = JsonSerializer.Serialize(new UpdateStoreInfoCommand(
                Profile.StoreName, 
                Profile.LegalName, 
                Profile.TaxRegistrationNumber,
                Profile.ContactEmail, 
                Profile.ContactPhone, 
                Profile.InvoiceFooterText));

            _originalStoreAddressJson = JsonSerializer.Serialize(Profile.StoreAddress);

            _originalRegionalSettingsJson = JsonSerializer.Serialize(new UpdateRegionalSettingsCommand(
                Profile.DefaultCurrency, 
                Profile.TimeZone, 
                Profile.WeightUnit));

            Sequences.Clear();
            var ordValue = await _settingsApi.GetSequenceValue("ORD");
            Sequences.Add(new AppSequenceViewModel 
            { 
                Id = "ORD", 
                DisplayName = "Order Number (ORD)", 
                CurrentValue = ordValue,
                CanEdit = CanEditAppSequences,
                OnCancelEdit = () => { } // Remove LoadDataAsync here since it's locally restored
            });

            var prodValue = await _settingsApi.GetSequenceValue("PROD");
            Sequences.Add(new AppSequenceViewModel
            { 
                Id = "PROD", 
                DisplayName = "Product SKU (PROD)", 
                CurrentValue = prodValue,
                CanEdit = CanEditAppSequences,
                OnCancelEdit = () => { } // Remove LoadDataAsync here since it's locally restored
            });

            if (!string.IsNullOrWhiteSpace(Profile.LogoUrl))
            {
                var imageBytes = await _fileService.ReadFileAsync("StoreAssets", Profile.LogoUrl);
                if (imageBytes != null)
                {
                    using var memStream = new MemoryStream(imageBytes);
                    using var randomAccessStream = memStream.AsRandomAccessStream();
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(randomAccessStream);
                    LogoImageSource = bitmapImage;
                }
            }

            await WhatsApp.LoadSettingsAsync();
            WhatsApp.CanEdit = CanEditTenantSettings;

            await PaymentMethods.LoadDataCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tenant profile or sequences.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void EditStoreInfo()
    {
        // Store original before edit in case we need to re-initialize
        _originalStoreInfoJson = JsonSerializer.Serialize(new UpdateStoreInfoCommand(
                Profile.StoreName,
                Profile.LegalName,
                Profile.TaxRegistrationNumber,
                Profile.ContactEmail,
                Profile.ContactPhone,
                Profile.InvoiceFooterText));
        IsEditingStoreInfo = true;
    }

    [RelayCommand]
    private void CancelEditStoreInfo()
    {
        if (!string.IsNullOrEmpty(_originalStoreInfoJson))
        {
            var backup = JsonSerializer.Deserialize<UpdateStoreInfoCommand>(_originalStoreInfoJson);
            if (backup != null)
            {
                Profile.StoreName = backup.StoreName;
                Profile.LegalName = backup.LegalName;
                Profile.TaxRegistrationNumber = backup.TaxRegistrationNumber;
                Profile.ContactEmail = backup.ContactEmail;
                Profile.ContactPhone = backup.ContactPhone;
            }
        }
        IsEditingStoreInfo = false;
    }

    [RelayCommand]
    private async Task SaveStoreInfoAsync()
    {
        if (Profile == null) return;
        try
        {
            IsBusy = true;
            var updateDto = new UpdateStoreInfoCommand(
                Profile.StoreName,
                Profile.LegalName,
                Profile.TaxRegistrationNumber,
                Profile.ContactEmail,
                Profile.ContactPhone,
                Profile.InvoiceFooterText);
            await _settingsApi.UpdateStoreInfo(updateDto);
            _toastService.ShowSuccess("Success", "Store info updated successfully.");
            
            // Update the baseline JSON
            _originalStoreInfoJson = JsonSerializer.Serialize(updateDto);
            IsEditingStoreInfo = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tenant store info.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void EditStoreAddress() 
    {
        _originalStoreAddressJson = JsonSerializer.Serialize(Profile.StoreAddress ?? new AddressDto());
        IsEditingStoreAddress = true;
    }

    [RelayCommand]
    private void CancelEditStoreAddress()
    {
        if (!string.IsNullOrEmpty(_originalStoreAddressJson))
        {
            var backup = JsonSerializer.Deserialize<AddressDto>(_originalStoreAddressJson);
            if (backup != null)
            {
                Profile.StoreAddress = backup;
            }
        }
        IsEditingStoreAddress = false;
    }

    [RelayCommand]
    private async Task SaveStoreAddressAsync()
    {
        if (Profile == null || Profile.StoreAddress == null) return;
        try
        {
            IsBusy = true;
            var updateDto = new UpdateStoreAddressCommand(Profile.StoreAddress);
            await _settingsApi.UpdateStoreAddress(updateDto);
            _toastService.ShowSuccess("Success", "Store address updated successfully.");
            
            // Update local baseline
            _originalStoreAddressJson = JsonSerializer.Serialize(Profile.StoreAddress);
            IsEditingStoreAddress = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tenant store address.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UploadLogoAsync()
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            // WinUI 3 Window Handle Hook
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            IsBusy = true;
            string extension = Path.GetExtension(file.Path);
            string newFileName = $"logo_{Guid.NewGuid()}{extension}";
            // Save file locally using your service
            using var stream = await file.OpenStreamForReadAsync();
            await _fileService.SaveImageAsync("StoreAssets", newFileName, stream);
            // Load preview into UI
            var imageBytes = await _fileService.ReadFileAsync("StoreAssets", newFileName);
            if (imageBytes != null)
            {
                using var memStream = new MemoryStream(imageBytes);
                using var randomAccessStream = memStream.AsRandomAccessStream();
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(randomAccessStream);

                LogoImageSource = bitmapImage;
            }
            // Sync with API
            await _settingsApi.UpdateLogoImage(new UpdateTenantLogoCommand(newFileName));
            Profile.LogoUrl = newFileName;

            _toastService.ShowSuccess("Success", "Logo updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload logo.");
            //_toastService.ShowError("Error", "Failed to upload logo.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteLogoAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Profile.LogoUrl)) return;
            IsBusy = true;

            // Clear in Backend
            await _settingsApi.UpdateLogoImage(new UpdateTenantLogoCommand(string.Empty));

            // Remove locally
            await _fileService.DeleteFileAsync("StoreAssets", Profile.LogoUrl);

            Profile.LogoUrl = string.Empty;
            LogoImageSource = null;

            _toastService.ShowSuccess("Success", "Logo removed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove logo.");
            //_toastService.ShowError("Error", "Failed to remove logo.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private void EditRegionalSettings()
    {
        _originalRegionalSettingsJson = JsonSerializer.Serialize(new UpdateRegionalSettingsCommand(
                Profile.DefaultCurrency,
                Profile.TimeZone,
                Profile.WeightUnit));
        IsEditingRegionalSettings = true;
    }

    [RelayCommand]
    private void CancelEditRegionalSettings()
    {
        if (!string.IsNullOrEmpty(_originalRegionalSettingsJson))
        {
            var backup = JsonSerializer.Deserialize<UpdateRegionalSettingsCommand>(_originalRegionalSettingsJson);
            if (backup != null)
            {
                Profile.DefaultCurrency = backup.DefaultCurrency;
                Profile.TimeZone = backup.TimeZone;
                Profile.WeightUnit = backup.WeightUnit;
            }
        }
        IsEditingRegionalSettings = false;
    }

    [RelayCommand]
    private async Task SaveRegionalSettingsAsync()
    {
        if (Profile == null) return;
        try
        {
            IsBusy = true;
            var updateDto = new UpdateRegionalSettingsCommand(
                Profile.DefaultCurrency,
                Profile.TimeZone,
                Profile.WeightUnit);
            await _settingsApi.UpdateRegionalSettings(updateDto);
            _toastService.ShowSuccess("Success", "Regional settings updated successfully.");
            
            // update baseline json
            _originalRegionalSettingsJson = JsonSerializer.Serialize(updateDto);
            IsEditingRegionalSettings = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tenant regional settings.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveSequenceAsync(AppSequenceViewModel item)
    {
        if (item == null) return;
        try
        {
            item.IsSaving = true;
            item.IsEditing = false;
            IsBusy = true;
            await _settingsApi.UpdateSequenceValue(item.Id, (int)item.CurrentValue);
            _toastService.ShowSuccess("Success", $"{item.DisplayName} updated successfully.");
            
            // Original values will be reset implicitly if we wanted to refetch, 
            // but the AppSequenceItem handles its own backup logic automatically upon re-Edit.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update app sequence values.");
        }
        finally
        {
            item.IsSaving = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme theme)
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

// Helpers
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
        return "Onyx OMS"; 
    }
}
