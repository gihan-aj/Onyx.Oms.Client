using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Features.Settings.Models;
using Onyx.Oms.Client.Desktop.Features.Settings.Services;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.Services.Http;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Onyx.Oms.Client.Desktop.Features.Settings;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ITenantProfileApi _tenantProfileApi;
    private readonly IAppSequenceApi _sequenceApi;
    private readonly IToastService _toastService;
    private readonly IPermissionService _permissionService;

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

    private TenantProfileDto _profile = new TenantProfileDto { StoreAddress = new AddressDto() };
    public TenantProfileDto Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    public ObservableCollection<AppSequenceItem> Sequences { get; } = new();

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        ITenantProfileApi tenantProfileApi,
        IAppSequenceApi sequenceApi,
        IToastService toastService,
        IPermissionService permissionService)
    {
        _themeSelectorService = themeSelectorService;
        _tenantProfileApi = tenantProfileApi;
        _sequenceApi = sequenceApi;
        _toastService = toastService;
        _permissionService = permissionService;

        _currentTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
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
            Profile = await _tenantProfileApi.GetTenantProfile();
            
            if (Profile.StoreAddress == null)
            {
                Profile.StoreAddress = new AddressDto();
            }

            IsEditingStoreInfo = false;
            IsEditingStoreAddress = false;
            IsEditingRegionalSettings = false;

            _originalStoreInfoJson = JsonSerializer.Serialize(new UpdateStoreInfoDto
            {
                StoreName = Profile.StoreName,
                LegalName = Profile.LegalName,
                TaxRegistrationNumber = Profile.TaxRegistrationNumber,
                ContactEmail = Profile.ContactEmail,
                ContactPhone = Profile.ContactPhone
            });

            _originalStoreAddressJson = JsonSerializer.Serialize(Profile.StoreAddress);

            _originalRegionalSettingsJson = JsonSerializer.Serialize(new UpdateRegionalSettingsDto
            {
                BaseCurrency = Profile.BaseCurrency,
                WeightUnit = Profile.WeightUnit
            });

            Sequences.Clear();
            var ordValue = await _sequenceApi.GetSequenceValue("ORD");
            Sequences.Add(new AppSequenceItem 
            { 
                Id = "ORD", 
                DisplayName = "Order Number (ORD)", 
                CurrentValue = ordValue,
                CanEdit = CanEditAppSequences,
                OnCancelEdit = () => { } // Remove LoadDataAsync here since it's locally restored
            });

            var prodValue = await _sequenceApi.GetSequenceValue("PROD");
            Sequences.Add(new AppSequenceItem 
            { 
                Id = "PROD", 
                DisplayName = "Product SKU (PROD)", 
                CurrentValue = prodValue,
                CanEdit = CanEditAppSequences,
                OnCancelEdit = () => { } // Remove LoadDataAsync here since it's locally restored
            });
        }
        catch (Exception)
        {
            // Error handling via ProblemDetailsHandler, just reset state
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
        _originalStoreInfoJson = JsonSerializer.Serialize(new UpdateStoreInfoDto
        {
            StoreName = Profile.StoreName,
            LegalName = Profile.LegalName,
            TaxRegistrationNumber = Profile.TaxRegistrationNumber,
            ContactEmail = Profile.ContactEmail,
            ContactPhone = Profile.ContactPhone
        });
        IsEditingStoreInfo = true;
    }

    [RelayCommand]
    private void CancelEditStoreInfo()
    {
        if (!string.IsNullOrEmpty(_originalStoreInfoJson))
        {
            var backup = JsonSerializer.Deserialize<UpdateStoreInfoDto>(_originalStoreInfoJson);
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
            IsLoading = true;
            var updateDto = new UpdateStoreInfoDto
            {
                StoreName = Profile.StoreName,
                LegalName = Profile.LegalName,
                TaxRegistrationNumber = Profile.TaxRegistrationNumber,
                ContactEmail = Profile.ContactEmail,
                ContactPhone = Profile.ContactPhone
            };
            await _tenantProfileApi.UpdateStoreInfo(updateDto);
            _toastService.ShowSuccess("Success", "Store info updated successfully.");
            
            // Update the baseline JSON
            _originalStoreInfoJson = JsonSerializer.Serialize(updateDto);
            IsEditingStoreInfo = false;
        }
        catch (Exception)
        { }
        finally
        {
            IsLoading = false;
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
            IsLoading = true;
            var updateDto = new UpdateStoreAddressDto { StoreAddress = Profile.StoreAddress };
            await _tenantProfileApi.UpdateStoreAddress(updateDto);
            _toastService.ShowSuccess("Success", "Store address updated successfully.");
            
            // Update local baseline
            _originalStoreAddressJson = JsonSerializer.Serialize(Profile.StoreAddress);
            IsEditingStoreAddress = false;
        }
        catch (Exception)
        { }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void EditRegionalSettings()
    {
        _originalRegionalSettingsJson = JsonSerializer.Serialize(new UpdateRegionalSettingsDto
        {
            BaseCurrency = Profile.BaseCurrency,
            WeightUnit = Profile.WeightUnit
        });
        IsEditingRegionalSettings = true;
    }

    [RelayCommand]
    private void CancelEditRegionalSettings()
    {
        if (!string.IsNullOrEmpty(_originalRegionalSettingsJson))
        {
            var backup = JsonSerializer.Deserialize<UpdateRegionalSettingsDto>(_originalRegionalSettingsJson);
            if (backup != null)
            {
                Profile.BaseCurrency = backup.BaseCurrency;
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
            IsLoading = true;
            var updateDto = new UpdateRegionalSettingsDto
            {
                BaseCurrency = Profile.BaseCurrency,
                WeightUnit = Profile.WeightUnit
            };
            await _tenantProfileApi.UpdateRegionalSettings(updateDto);
            _toastService.ShowSuccess("Success", "Regional settings updated successfully.");
            
            // update baseline json
            _originalRegionalSettingsJson = JsonSerializer.Serialize(updateDto);
            IsEditingRegionalSettings = false;
        }
        catch (Exception)
        { }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSequenceAsync(AppSequenceItem item)
    {
        if (item == null) return;
        try
        {
            item.IsSaving = true;
            item.IsEditing = false;
            await _sequenceApi.UpdateSequenceValue(item.Id, (int)item.CurrentValue);
            _toastService.ShowSuccess("Success", $"{item.DisplayName} updated successfully.");
            
            // Original values will be reset implicitly if we wanted to refetch, 
            // but the AppSequenceItem handles its own backup logic automatically upon re-Edit.
        }
        catch (Exception)
        { }
        finally
        {
            item.IsSaving = false;
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
