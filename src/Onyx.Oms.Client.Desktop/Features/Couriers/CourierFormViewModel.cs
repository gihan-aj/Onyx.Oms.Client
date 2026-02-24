using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public partial class CourierFormViewModel : ObservableObject, INavigationAware
{
    private readonly ICourierApi _courierApi;
    private readonly IToastService _toastService;
    private readonly ILogger<CourierFormViewModel> _logger;
    private readonly INavigationService _navigationService;

    public bool IsEditMode { get; private set; }
    public Guid? CourierId { get; private set; }

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

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

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
    }

    public async Task InitializeAsync(CourierDto? courierToEdit = null)
    {
        IsLoading = true;
        try
        {
            if (courierToEdit != null)
            {
                IsEditMode = true;
                CourierId = courierToEdit.Id;
                Name = courierToEdit.Name ?? string.Empty;
                ContactPerson = courierToEdit.ContactPerson;
                PrimaryPhone = courierToEdit.PrimaryPhone;
                SecondaryPhone = courierToEdit.SecondaryPhone;
                WebsiteUrl = courierToEdit.WebsiteUrl;
                IsActive = courierToEdit.IsActive;
                Title = $"Edit Courier ({Name})";
            }
            else
            {
                IsEditMode = false;
                Title = "Create Courier";
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
        IsLoading = true;
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
                    IsActive = IsActive
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
                    IsActive = IsActive
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
            IsLoading = false;
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
}
