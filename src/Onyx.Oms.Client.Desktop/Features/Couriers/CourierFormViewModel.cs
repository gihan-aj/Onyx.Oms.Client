using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public partial class CourierFormViewModel : ObservableObject, INavigationAware
{
    private readonly ICourierApi _courierApi;
    private readonly IToastService _toastService;
    private readonly ILogger<CourierFormViewModel> _logger;
    private readonly INavigationService _navigationService;

    private bool _isEditMode;
    public bool IsEditMode { get => _isEditMode; private set => SetProperty(ref _isEditMode, value); }
    
    private Guid? _courierId;
    public Guid? CourierId { get => _courierId; private set => SetProperty(ref _courierId, value); }

    private string _title = "Create Courier";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _contactPerson;
    public string? ContactPerson
    {
        get => _contactPerson;
        set => SetProperty(ref _contactPerson, value);
    }

    private string? _primaryPhone;
    public string? PrimaryPhone
    {
        get => _primaryPhone;
        set => SetProperty(ref _primaryPhone, value);
    }

    private string? _secondaryPhone;
    public string? SecondaryPhone
    {
        get => _secondaryPhone;
        set => SetProperty(ref _secondaryPhone, value);
    }

    private string? _websiteUrl;
    public string? WebsiteUrl
    {
        get => _websiteUrl;
        set => SetProperty(ref _websiteUrl, value);
    }

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // Validation errors (if inline validation is used later)
    private string? _nameError;
    public string? NameError
    {
        get => _nameError;
        set => SetProperty(ref _nameError, value);
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

    /// <summary>The zone rates being configured. Bound to the Zone Rates ListView.</summary>
    public ObservableCollection<ZoneRateFormItem> ZoneRates { get; } = new();

    private bool _hasZoneRates;
    public bool HasZoneRates
    {
        get => _hasZoneRates;
        set => SetProperty(ref _hasZoneRates, value);
    }

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand<ZoneRateFormItem> RemoveZoneRateCommand { get; }

    public CourierFormViewModel(
        ICourierApi courierApi,
        IToastService toastService,
        ILogger<CourierFormViewModel> logger,
        INavigationService navigationService)
    {
        _courierApi = courierApi;
        _toastService = toastService;
        _logger = logger;
        _navigationService = navigationService;

        SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
        CancelCommand = new RelayCommand(OnCancelExecute);
        RemoveZoneRateCommand = new RelayCommand<ZoneRateFormItem>(OnRemoveZoneRate);

        ZoneRates.CollectionChanged += (s, e) =>
        {
            HasZoneRates = ZoneRates.Count > 0;
        };
    }

    public async Task InitializeAsync(CourierDto? courierToEdit = null)
    {
        IsLoading = true;
        try
        {
            if (courierToEdit != null)
            {
                IsEditMode = true;
                // Fetch full details (includes ZoneRates)
                var courierDetails = await _courierApi.GetCourier(courierToEdit.Id);

                CourierId = courierDetails.Id;
                Name = courierDetails.Name ?? string.Empty;
                ContactPerson = courierDetails.ContactPerson;
                PrimaryPhone = courierDetails.PrimaryPhone;
                SecondaryPhone = courierDetails.SecondaryPhone;
                WebsiteUrl = courierDetails.WebsiteUrl;
                IsActive = courierDetails.IsActive;
                Title = $"Edit Courier ({Name})";

                // Map zone rates from server
                ZoneRates.Clear();
                foreach (var zr in courierDetails.ZoneRates)
                {
                    ZoneRates.Add(new ZoneRateFormItem
                    {
                        Id = zr.Id,
                        ZoneName = zr.ZoneName,
                        BaseFee = zr.BaseFee,
                        BaseWeight = zr.BaseWeight,
                        ExcessFeePerWeightUnit = zr.ExcessFeePerWeightUnit,
                        CodPercentage = zr.CodPercentage,
                        Currency = zr.BaseFeeCurrency.Length > 0 ? zr.BaseFeeCurrency : "LKR",
                        WeightUnit = zr.BaseWeightUnit.Length > 0 ? zr.BaseWeightUnit : "kg",
                        IsDefault = zr.IsDefault,
                        CoveredDistricts = new ObservableCollection<string>(zr.CoveredDistricts)
                    });
                }
            }
            else
            {
                IsEditMode = false;
                Title = "Create Courier";

                // Seed two default zone rates as per spec (user can edit before saving)
                ZoneRates.Clear();
                ZoneRates.Add(new ZoneRateFormItem
                {
                    ZoneName = "Colombo",
                    BaseFee = 0,
                    BaseWeight = 1,
                    Currency = "LKR",
                    WeightUnit = "kg",
                    IsDefault = false,
                    CoveredDistricts = new ObservableCollection<string> { "Colombo" }
                });
                ZoneRates.Add(new ZoneRateFormItem
                {
                    ZoneName = "Outstation",
                    BaseFee = 0,
                    BaseWeight = 1,
                    Currency = "LKR",
                    WeightUnit = "kg",
                    IsDefault = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize courier form");
        }
        finally
        {
            IsLoading = false;
        }

        // Must await due to async Task signature, even if synchronous inside
        await Task.CompletedTask;
    }

    private void OnCancelExecute()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private async Task OnSaveExecuteAsync()
    {
        var result = await SaveAsync();
        if (result && _navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    public async Task<bool> SaveAsync()
    {
        IsBusy = true;
        NameError = null;

        try
        {
            if (IsEditMode)
            {
                var updateDto = new UpdateCourierDto
                {
                    Id = CourierId!.Value,
                    Name = Name,
                    ContactPerson = ContactPerson,
                    PrimaryPhone = PrimaryPhone,
                    SecondaryPhone = SecondaryPhone,
                    WebsiteUrl = WebsiteUrl,
                    IsActive = IsActive,
                    ZoneRates = ZoneRates.Select(zr => new UpdateCourierZoneRateDto
                    {
                        Id = zr.Id,              // null for newly added rows
                        ZoneName = zr.ZoneName,
                        BaseFee = zr.BaseFee,
                        BaseWeight = zr.BaseWeight,
                        ExcessFeePerWeightUnit = zr.ExcessFeePerWeightUnit,
                        CodPercentage = zr.CodPercentage,
                        Currency = zr.Currency,
                        WeightUnit = zr.WeightUnit,
                        IsDefault = zr.IsDefault,
                        CoveredDistricts = zr.CoveredDistricts.ToList()
                    }).ToList()
                };
                await _courierApi.UpdateCourier(updateDto.Id, updateDto);
                _toastService.ShowSuccess("Success", "Courier updated successfully.");
            }
            else
            {
                var createDto = new CreateCourierDto
                {
                    Name = Name,
                    ContactPerson = ContactPerson,
                    PrimaryPhone = PrimaryPhone,
                    SecondaryPhone = SecondaryPhone,
                    WebsiteUrl = WebsiteUrl,
                    IsActive = IsActive,
                    ZoneRates = ZoneRates.Select(zr => new CreateCourierZoneRateDto
                    {
                        ZoneName = zr.ZoneName,
                        BaseFee = zr.BaseFee,
                        BaseWeight = zr.BaseWeight,
                        ExcessFeePerWeightUnit = zr.ExcessFeePerWeightUnit,
                        CodPercentage = zr.CodPercentage,
                        Currency = zr.Currency,
                        WeightUnit = zr.WeightUnit,
                        IsDefault = zr.IsDefault,
                        CoveredDistricts = zr.CoveredDistricts.ToList()
                    }).ToList()
                };
                await _courierApi.CreateCourier(createDto);
                _toastService.ShowSuccess("Success", "Courier created successfully.");
            }
            return true;
        }
        catch (Refit.ApiException ex)
        {
            var problemDetails = await ex.GetContentAsAsync<Shared.Models.ProblemDetails>();
            var errors = problemDetails?.Errors ?? problemDetails?.Extensions?.Errors;

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    if (string.Equals(error.Code, "Name", StringComparison.OrdinalIgnoreCase) ||
                        error.Description?.Contains("Name", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        NameError = error.Description;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save courier");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        // Parameter might be the CourierDto if passed from navigation
        if (parameter is CourierDto dto)
        {
            await InitializeAsync(dto);
        }
        else
        {
            await InitializeAsync();
        }
    }

    public void OnNavigatedFrom()
    {
    }

    // ── Zone Rate helpers (called from code-behind, which owns XamlRoot) ──

    private void OnRemoveZoneRate(ZoneRateFormItem? item)
    {
        if (item != null && ZoneRates.Count > 1)
            ZoneRates.Remove(item);
    }

    /// <summary>
    /// Called after the Add/Edit dialog confirms a zone with IsDefault = true.
    /// Clears the IsDefault flag on all other zones so only one default exists.
    /// </summary>
    public void EnsureSingleDefault(ZoneRateFormItem confirmed)
    {
        if (!confirmed.IsDefault) return;
        foreach (var zr in ZoneRates.Where(zr => zr != confirmed))
            zr.IsDefault = false;
    }
}
