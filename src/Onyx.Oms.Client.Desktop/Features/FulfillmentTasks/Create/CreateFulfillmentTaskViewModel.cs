using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.Create
{
    public partial class CreateFulfillmentTaskViewModel : ObservableObject, INavigationAware
    {
        private readonly IFulfillmentTasksApi _taskApi;
        private readonly IToastService _toastService;
        private readonly ILogger<CreateFulfillmentTaskViewModel> _logger;
        private readonly INavigationService _navigationService;
        private readonly IFileService _fileService;
        private readonly ITenantProfileService _tenantProfileService;

        public string Title => "Create Fulfillment Task";

        // --- Form Swapper Logic ---
        private int _selectedTaskTypeIndex = 0; // 0 = Production, 1 = Procurement
        public int SelectedTaskTypeIndex
        {
            get => _selectedTaskTypeIndex;
            set
            {
                if (SetProperty(ref _selectedTaskTypeIndex, value))
                {
                    OnPropertyChanged(nameof(IsProductionFormVisible));
                    OnPropertyChanged(nameof(IsProcurementFormVisible));
                    ClearErrors(); // Clear validation errors when swapping forms
                }
            }
        }

        public bool IsProductionFormVisible => SelectedTaskTypeIndex == 0;
        public bool IsProcurementFormVisible => SelectedTaskTypeIndex == 1;

        // --- Shared Fields ---
        private Guid? _productVariantId;
        public Guid? ProductVariantId { get => _productVariantId; set => SetProperty(ref _productVariantId, value); }
        private string? _productVariantIdError;
        public string? ProductVariantIdError { get => _productVariantIdError; set => SetProperty(ref _productVariantIdError, value); }

        // Placeholder until the Product Dialog is implemented
        private string _productVariantName = string.Empty;
        public string ProductVariantName { get => _productVariantName; set => SetProperty(ref _productVariantName, value); }

        private double _requestedQuantity = 1; // NumberBox binds to double
        public double RequestedQuantity { get => _requestedQuantity; set => SetProperty(ref _requestedQuantity, value); }
        private string? _requestedQuantityError;
        public string? RequestedQuantityError { get => _requestedQuantityError; set => SetProperty(ref _requestedQuantityError, value); }

        public IReadOnlyList<TaskPriority> Priorities { get; } = Enum.GetValues<TaskPriority>().ToList();

        private TaskPriority _selectedPriority = TaskPriority.Normal;
        public TaskPriority SelectedPriority { get => _selectedPriority; set => SetProperty(ref _selectedPriority, value); }

        private DateTimeOffset? _expectedCompletionDate;
        public DateTimeOffset? ExpectedCompletionDate { get => _expectedCompletionDate; set => SetProperty(ref _expectedCompletionDate, value); }

        private string? _notes;
        public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

        // --- Production Fields ---
        private Guid? _assignedUserId;
        public Guid? AssignedUserId { get => _assignedUserId; set => SetProperty(ref _assignedUserId, value); }

        public List<UserDto> AvailableUsers { get; private set; } = new(); // Populate this from your user API

        // --- Procurement Fields ---
        private string _purchaseOrderNumber = string.Empty;
        public string PurchaseOrderNumber { get => _purchaseOrderNumber; set => SetProperty(ref _purchaseOrderNumber, value); }
        private string? _purchaseOrderNumberError;
        public string? PurchaseOrderNumberError { get => _purchaseOrderNumberError; set => SetProperty(ref _purchaseOrderNumberError, value); }

        private string _baseCurrency = "LKR";
        public string BaseCurrency { get => _baseCurrency; set => SetProperty(ref _baseCurrency, value); }

        private double _costAmount = 0;
        public double CostAmount { get => _costAmount; set => SetProperty(ref _costAmount, value); }
        private string? _costAmountError;
        public string? CostAmountError { get => _costAmountError; set => SetProperty(ref _costAmountError, value); }


        // --- UI State ---
        private bool _isLoading = true;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand ShowProductPickerCommand { get; }

        public CreateFulfillmentTaskViewModel(
            IFulfillmentTasksApi taskApi,
            IToastService toastService,
            ILogger<CreateFulfillmentTaskViewModel> logger,
            INavigationService navigationService,
            IFileService fileService,
            ITenantProfileService tenantProfileService)
        {
            _taskApi = taskApi;
            _toastService = toastService;
            _logger = logger;
            _navigationService = navigationService;
            _fileService = fileService;
            _tenantProfileService = tenantProfileService;

            SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
            CancelCommand = new RelayCommand(OnCancelExecute);
            ShowProductPickerCommand = new AsyncRelayCommand(OnShowProductPickerExecuteAsync);
        }

        public void OnNavigatedFrom()
        {
            
        }

        public void OnNavigatedTo(object parameter)
        {
            IsLoading = true;
            try
            {
                // Optionally load available users for the assignee dropdown here
                // AvailableUsers = await _userApi.GetProductionWorkersAsync();

                // If the user navigated here from a specific order/product, handle the parameter mapping
                if (parameter is Guid preSelectedVariantId)
                {
                    ProductVariantId = preSelectedVariantId;
                    // Fetch variant name...
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize task form");
            }
            finally
            {
                IsLoading = false;
            }

            BaseCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
        }

        private void OnCancelExecute()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        private async Task OnShowProductPickerExecuteAsync()
        {
            var dialog = new ProductPicker.ProductPickerDialog();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            
            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.ViewModel.SelectedItem != null)
            {
                ProductVariantId = dialog.ViewModel.SelectedItem.ResolvedVariant?.Id;
                ProductVariantName = dialog.ViewModel.SelectedItem.DisplayName;
                ProductVariantIdError = null;
            }
        }

        private void ClearErrors()
        {
            ProductVariantIdError = null;
            RequestedQuantityError = null;
            PurchaseOrderNumberError = null;
            CostAmountError = null;
        }

        private async Task OnSaveExecuteAsync()
        {
            IsBusy = true;
            ClearErrors();

            try
            {
                // Basic UI fallback validation before sending to API
                if (ProductVariantId == null || ProductVariantId == Guid.Empty)
                {
                    ProductVariantIdError = "Please select a product variant.";
                    return;
                }

                if (IsProductionFormVisible)
                {
                    var command = new CreateProductionTaskCommand(
                        ProductVariantId.Value,
                        (int)RequestedQuantity,
                        AssignedUserId,
                        Notes,
                        ExpectedCompletionDate,
                        SelectedPriority
                    );

                    await _taskApi.CreateProductionTask(command);
                    _toastService.ShowSuccess("Success", "Production task created successfully.");
                }
                else // IsProcurementFormVisible
                {
                    var command = new CreateProcurementTaskCommand(
                        ProductVariantId.Value,
                        (int)RequestedQuantity,
                        new MoneyDto((decimal)CostAmount, BaseCurrency),
                        PurchaseOrderNumber,
                        Notes,
                        ExpectedCompletionDate,
                        SelectedPriority
                    );

                    await _taskApi.CreateProcurementTask(command);
                    _toastService.ShowSuccess("Success", "Procurement task created successfully.");
                }

                if (_navigationService.CanGoBack)
                {
                    _navigationService.GoBack();
                }
            }
            catch (Refit.ApiException ex)
            {
                var problemDetails = await ex.GetContentAsAsync<Shared.Models.ProblemDetails>();
                var errors = problemDetails?.Errors ?? problemDetails?.Extensions?.Errors;

                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        if (error.Code!.Contains("ProductVariantId", StringComparison.OrdinalIgnoreCase))
                            ProductVariantIdError = error.Description;
                        else if (error.Code.Contains("RequestedQuantity", StringComparison.OrdinalIgnoreCase))
                            RequestedQuantityError = error.Description;
                        else if (error.Code.Contains("PurchaseOrderNumber", StringComparison.OrdinalIgnoreCase))
                            PurchaseOrderNumberError = error.Description;
                        else if (error.Code.Contains("Cost", StringComparison.OrdinalIgnoreCase))
                            CostAmountError = error.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create task");
                _toastService.ShowError("Error", "An unexpected error occurred while creating the task.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
