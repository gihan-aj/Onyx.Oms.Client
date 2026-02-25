using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public partial class CustomerFormViewModel : ObservableObject, INavigationAware
{
    private readonly ICustomerApi _customerApi;
    private readonly IToastService _toastService;
    private readonly ILogger<CustomerFormViewModel> _logger;
    private readonly INavigationService _navigationService;

    public bool IsEditMode { get; private set; }
    public Guid? CustomerId { get; private set; }

    private bool _isReadOnly;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        private set
        {
            if (SetProperty(ref _isReadOnly, value))
            {
                OnPropertyChanged(nameof(IsNotReadOnly));
            }
        }
    }
    public bool IsNotReadOnly => !IsReadOnly;

    private string _title = "Create Customer";
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    private string? _nameError;
    public string? NameError { get => _nameError; set => SetProperty(ref _nameError, value); }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    private string? _emailError;
    public string? EmailError { get => _emailError; set => SetProperty(ref _emailError, value); }

    private string _primaryPhone = string.Empty;
    public string PrimaryPhone { get => _primaryPhone; set => SetProperty(ref _primaryPhone, value); }
    private string? _primaryPhoneError;
    public string? PrimaryPhoneError { get => _primaryPhoneError; set => SetProperty(ref _primaryPhoneError, value); }

    private string? _secondaryPhone;
    public string? SecondaryPhone { get => _secondaryPhone; set => SetProperty(ref _secondaryPhone, value); }

    private string _street = string.Empty;
    public string Street { get => _street; set => SetProperty(ref _street, value); }

    private string _city = string.Empty;
    public string City { get => _city; set => SetProperty(ref _city, value); }

    private string _state = string.Empty;
    public string State { get => _state; set => SetProperty(ref _state, value); }

    private string _postalCode = string.Empty;
    public string PostalCode { get => _postalCode; set => SetProperty(ref _postalCode, value); }

    private string _country = string.Empty;
    public string Country { get => _country; set => SetProperty(ref _country, value); }

    private string? _notes;
    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

    private bool _isLoading = true;
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public CustomerFormViewModel(
        ICustomerApi customerApi, 
        IToastService toastService, 
        ILogger<CustomerFormViewModel> logger,
        INavigationService navigationService)
    {
        _customerApi = customerApi;
        _toastService = toastService;
        _logger = logger;
        _navigationService = navigationService;

        SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
        CancelCommand = new RelayCommand(OnCancelExecute);
    }

    public async Task InitializeAsync(CustomerDto? customerToEdit = null, bool isReadOnly = false)
    {
        IsReadOnly = isReadOnly;
        IsLoading = true;
        try
        {
            if (customerToEdit != null)
            {
                IsEditMode = !IsReadOnly;
                CustomerId = customerToEdit.Id;
                Name = customerToEdit.Name;
                Email = customerToEdit.Email;
                PrimaryPhone = customerToEdit.PrimaryPhone;
                SecondaryPhone = customerToEdit.SecondaryPhone;
                
                if (customerToEdit.Address != null)
                {
                    Street = customerToEdit.Address.Street;
                    City = customerToEdit.Address.City;
                    State = customerToEdit.Address.State;
                    PostalCode = customerToEdit.Address.PostalCode;
                    Country = customerToEdit.Address.Country;
                }
                
                Notes = customerToEdit.Notes;
                Title = IsReadOnly ? $"Customer Details ({Name})" : $"Edit Customer ({Name})";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize customer form");
        }
        finally
        {
            IsLoading = false;
        }
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
        EmailError = null;
        PrimaryPhoneError = null;
        
        try
        {
            if (IsEditMode)
            {
                var updateDto = new UpdateCustomerDto
                {
                    Id = CustomerId!.Value,
                    Name = Name,
                    Email = Email,
                    PrimaryPhone = PrimaryPhone,
                    SecondaryPhone = SecondaryPhone,
                    Street = Street,
                    City = City,
                    State = State,
                    PostalCode = PostalCode,
                    Country = Country,
                    Notes = Notes
                };
                await _customerApi.UpdateCustomer(updateDto.Id, updateDto);
                _toastService.ShowSuccess("Success", "Customer updated successfully.");
            }
            else
            {
                var createDto = new CreateCustomerDto
                {
                    Name = Name,
                    Email = Email,
                    PrimaryPhone = PrimaryPhone,
                    SecondaryPhone = SecondaryPhone,
                    Street = Street,
                    City = City,
                    State = State,
                    PostalCode = PostalCode,
                    Country = Country,
                    Notes = Notes
                };
                await _customerApi.CreateCustomer(createDto);
                _toastService.ShowSuccess("Success", "Customer created successfully.");
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
                    else if (string.Equals(error.Code, "Email", StringComparison.OrdinalIgnoreCase) || 
                             error.Description?.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        EmailError = error.Description;
                    }
                    else if (string.Equals(error.Code, "PrimaryPhone", StringComparison.OrdinalIgnoreCase) || 
                             error.Description?.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        PrimaryPhoneError = error.Description;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save customer");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is Guid customerId)
        {
            var customerDetails = await _customerApi.GetCustomerById(customerId);
            await InitializeAsync(customerDetails);
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
