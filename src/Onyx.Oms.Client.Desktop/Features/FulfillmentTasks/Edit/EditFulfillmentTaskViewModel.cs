using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.Edit
{
    public partial class EditFulfillmentTaskViewModel : ObservableObject, INavigationAware
    {
        private readonly IFulfillmentTasksApi _taskApi;
        private readonly IToastService _toastService;
        private readonly ILogger<EditFulfillmentTaskViewModel> _logger;
        private readonly INavigationService _navigationService;
        private readonly ITenantProfileService _tenantProfileService;

        private Guid _taskId;

        public string Title => $"Edit Task";

        // --- Shared Fields ---
        private string _taskType = string.Empty;
        public string TaskType { get => _taskType; set => SetProperty(ref _taskType, value); }
        
        private string _status = string.Empty;
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        private string _productVariantName = string.Empty;
        public string ProductVariantName { get => _productVariantName; set => SetProperty(ref _productVariantName, value); }

        private double _requestedQuantity = 1;
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
        public bool IsProductionFormVisible => TaskType == FulfillmentTaskType.Production.ToString();
        
        private Guid? _assignedUserId;
        public Guid? AssignedUserId { get => _assignedUserId; set => SetProperty(ref _assignedUserId, value); }

        public List<UserDto> AvailableUsers { get; private set; } = new();

        // --- Procurement Fields ---
        public bool IsProcurementFormVisible => TaskType == FulfillmentTaskType.Procurement.ToString();
        
        private bool _isProcurementCostVisible;
        public bool IsProcurementCostVisible { get => _isProcurementCostVisible; set => SetProperty(ref _isProcurementCostVisible, value); }

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

        public EditFulfillmentTaskViewModel(
            IFulfillmentTasksApi taskApi,
            IToastService toastService,
            ILogger<EditFulfillmentTaskViewModel> logger,
            INavigationService navigationService,
            ITenantProfileService tenantProfileService)
        {
            _taskApi = taskApi;
            _toastService = toastService;
            _logger = logger;
            _navigationService = navigationService;
            _tenantProfileService = tenantProfileService;

            SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
            CancelCommand = new RelayCommand(OnCancelExecute);
        }

        public void OnNavigatedFrom()
        {
        }

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is Guid taskId)
            {
                _taskId = taskId;
                await LoadDataAsync();
            }
            else
            {
                _toastService.ShowError("Error", "Invalid task ID provided.");
                if (_navigationService.CanGoBack) _navigationService.GoBack();
            }
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                BaseCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";

                // Load users if needed here (for production task assignment).
                // Example: AvailableUsers = await _userApi.GetProductionWorkersAsync();

                var taskDto = await _taskApi.GetFulfillmentTaskById(_taskId);
                
                TaskType = taskDto.Type.ToString();
                Status = taskDto.Status.ToString();
                ProductVariantName = taskDto.ProductName;
                
                RequestedQuantity = taskDto.RequestedQuantity;
                SelectedPriority = taskDto.Priority;
                ExpectedCompletionDate = taskDto.ExpectedCompletionDate;
                Notes = taskDto.Notes;

                OnPropertyChanged(nameof(IsProductionFormVisible));
                OnPropertyChanged(nameof(IsProcurementFormVisible));

                if (taskDto.Type == FulfillmentTaskType.Production)
                {
                    AssignedUserId = taskDto.AssignedUserId;
                }
                else if (taskDto.Type == FulfillmentTaskType.Procurement)
                {
                    IsProcurementCostVisible = taskDto.Status != FulfillmentTaskStatus.Pending;

                    PurchaseOrderNumber = taskDto.PurchaseOrderNumber ?? string.Empty;
                    if (taskDto.Cost != null)
                    {
                        CostAmount = (double)taskDto.Cost.Amount;
                        if (!string.IsNullOrEmpty(taskDto.Cost.Currency))
                        {
                            BaseCurrency = taskDto.Cost.Currency;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load task details for editing.");
                _toastService.ShowError("Error", "Failed to load task details.");
                if (_navigationService.CanGoBack) _navigationService.GoBack();
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

        private void ClearErrors()
        {
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
                if (IsProductionFormVisible)
                {
                    var command = new UpdateProductionTaskCommand(
                        _taskId,
                        (int)RequestedQuantity,
                        AssignedUserId,
                        ExpectedCompletionDate,
                        SelectedPriority,
                        Notes
                    );

                    await _taskApi.UpdateProduction(command);
                    _toastService.ShowSuccess("Success", "Production task updated successfully.");
                }
                else
                {
                    MoneyDto? costDto = null;
                    string? poNumber = null;

                    if (IsProcurementCostVisible)
                    {
                        costDto = new MoneyDto((decimal)CostAmount, BaseCurrency);
                        poNumber = PurchaseOrderNumber;
                    }

                    var command = new UpdateProcurementTaskCommand(
                        _taskId,
                        (int)RequestedQuantity,
                        poNumber,
                        costDto,
                        ExpectedCompletionDate,
                        SelectedPriority,
                        Notes
                    );

                    await _taskApi.UpdateProcurementn(command);
                    _toastService.ShowSuccess("Success", "Procurement task updated successfully.");
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
                        if (error.Code!.Contains("RequestedQuantity", StringComparison.OrdinalIgnoreCase))
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
                _logger.LogError(ex, "Failed to update task");
                _toastService.ShowError("Error", "An unexpected error occurred while updating the task.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
