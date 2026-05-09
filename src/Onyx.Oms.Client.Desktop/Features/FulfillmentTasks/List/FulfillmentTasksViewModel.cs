using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Features.Customers;
using Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.Create;
using Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.Edit;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Onyx.Oms.Client.Desktop.Shared.Constants.Permissions;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public partial class FulfillmentTasksViewModel : PagedDataGridViewModelBase<FulfillmentTaskGridItem>, INavigationAware
    {
        private readonly IFulfillmentTasksApi _api;
        private readonly IPermissionService _permissionService;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly IDialogService _dialogService;
        private readonly ITenantProfileService _tenantProfile;

        // Drafts
        private readonly Dictionary<Guid, int> _draftMarkReadyQuantities = new();
        private readonly Dictionary<Guid, int> _draftStartProductionQuantities = new();
        private readonly Dictionary<Guid, int> _draftIssuePoQuantities = new();
        private readonly Dictionary<Guid, string> _draftIssuePoNumbers = new();
        private readonly Dictionary<Guid, double> _draftIssuePoCosts = new();

        // Grouping
        private ObservableCollection<FulfillmentGroup> _groupedTasks = new ();
        public ObservableCollection<FulfillmentGroup> GroupedTasks
        {
            get => _groupedTasks;
            set => SetProperty(ref _groupedTasks, value);
        }

        // -- Filtering --
        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private FulfillmentTaskType? _selectedType;
        public FulfillmentTaskType? SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        public Array TypeOptions { get; } = Enum.GetValues(typeof(FulfillmentTaskType));

        private TaskPriority? _selectedPriority;
        public TaskPriority? SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                if (SetProperty(ref _selectedPriority, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        public Array PriorityOptions { get; } = Enum.GetValues(typeof(TaskPriority));

        private DateTimeOffset? _selectedDate;
        public DateTimeOffset? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private bool _showAllStatus;
        public bool ShowAllStatus
        {
            get => _showAllStatus;
            set
            {
                if (SetProperty(ref _showAllStatus, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private DateTimeOffset? _createdAfterFilter = DateTimeOffset.UtcNow.AddMonths(-1);
        public DateTimeOffset? CreatedAfterFilter
        {
            get => _createdAfterFilter;
            set
            {
                if (SetProperty(ref _createdAfterFilter, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // --- Permissions ---
        public bool CanCreateTasks => _permissionService.CanExecute(Permissions.FulfillmentTasks.Create);
        public bool CanEditTasks => _permissionService.CanExecute(Permissions.FulfillmentTasks.Edit);

        // -- Commands --
        public IAsyncRelayCommand ClearFiltersCommand { get; }
        public IRelayCommand NewTaskCommand { get; }
        public IAsyncRelayCommand EditTaskCommand { get; }
        public IAsyncRelayCommand StartWorkCommand { get; }
        public IAsyncRelayCommand IssuePoCommand { get; }
        public IAsyncRelayCommand MarkReadyCommand { get; }
        public IAsyncRelayCommand CancelTaskCommand { get; }
        public IAsyncRelayCommand ViewTaskDetailsCommand { get; }
        public IAsyncRelayCommand<FulfillmentGroup> CompleteBatchCommand { get; }

        public FulfillmentTasksViewModel(IFulfillmentTasksApi api, IPermissionService permissionService, INavigationService navigationService, IToastService toastService, IDialogService dialogService, ITenantProfileService tenantProfile)
        {
            _api = api;
            _permissionService = permissionService;
            _navigationService = navigationService;
            _toastService = toastService;
            _dialogService = dialogService;
            _tenantProfile = tenantProfile;

            ClearFiltersCommand = new AsyncRelayCommand(ClearFlitersAsync);
            NewTaskCommand = new RelayCommand(NavigateToNewTask);
            EditTaskCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(EditTaskAsync);
            StartWorkCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(StartWorkAsync);
            IssuePoCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(IssuePoAsync);
            MarkReadyCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(MarkReadyAsync);
            CancelTaskCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(CancelTaskAsync);
            ViewTaskDetailsCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(ViewTaskDetailsAsync);
            CompleteBatchCommand = new AsyncRelayCommand<FulfillmentGroup>(CompleteBatchAsync);
        }

        public void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadDataAsync();
        }

        protected override async Task LoadDataAsync()
        {
            if (IsListLoading)
                return;

            try
            {
                IsListLoading = true;

                var result = await _api.GetFulfillmentTasksPaged(
                    page: Page,
                    pageSize: PageSize,
                    searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    sortColumn: SortColumn,
                    sortOrder: SortOrder,
                    type: SelectedType,
                    priority: SelectedPriority,
                    expectedCompletionDate: SelectedDate,
                    showAllStatus: ShowAllStatus,
                    createdAfter: CreatedAfterFilter);

                Items.Clear();

                foreach (var item in result.Items)
                {
                    var gridItem = item.ToGridItem(CanEditTasks);
                    Items.Add(gridItem);
                }

                Page = result.Page;
                TotalCount = result.TotalCount;
                HasNextPage = result.HasNextPage;
                HasPreviousPage = result.HasPreviousPage;

                GroupTasksForUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
            }
            finally
            {
                IsListLoading = false;
            }
        }

        private void GroupTasksForUI()
        {
            var grouped = Items
                .GroupBy(t => t.ProductVariantId)
                .Select(g => new FulfillmentGroup(
                    g.Key,
                    g.First().ProductName,
                    g.First().Sku,
                    g.First().VariantAttributes,
                    g.Sum(t => t.RequestedQuantity),
                    g.Sum(t => t.CompletedQuantity),
                    CompleteBatchCommand,
                    g
                ));

            GroupedTasks = new ObservableCollection<FulfillmentGroup>(grouped);
        }

        protected override Task OnRefreshFiltersAsync()
        {
            SearchTerm = string.Empty;
            return Task.CompletedTask;
        }

        private async Task ClearFlitersAsync()
        {
            SearchTerm = string.Empty;
            SelectedType = null;
            SelectedPriority = null;
            SelectedDate = null;
            ShowAllStatus = false;
            CreatedAfterFilter = DateTimeOffset.UtcNow.AddMonths(-1);
            Page = 1;
            await LoadDataAsync();
        }

        private void NavigateToNewTask()
        {
            _navigationService.NavigateTo(typeof(CreateFulfillmentTaskPage).FullName!);
        }

        private async Task EditTaskAsync(FulfillmentTaskGridItem? task)
        {
            if (task == null)
                return;

            _navigationService.NavigateTo(typeof(EditFulfillmentTaskViewModel).FullName!, task.Id);
        }

        private async Task ViewTaskDetailsAsync(FulfillmentTaskGridItem? task)
        {
            if (task == null)
                return;

            var dialog = new FulfillmentTaskDetailsDialog(task)
            {
                XamlRoot = _dialogService.CurrentXamlRoot,
            };

            await dialog.ShowAsync();
        }

        private async Task StartWorkAsync(FulfillmentTaskGridItem? task)
        {
            if (task == null || task.Type == FulfillmentTaskType.Procurement)
                return;

            int remainingToStart = task.RequestedQuantity - task.StartedQuantity;
            if (remainingToStart <= 0)
                return;

            int initialValue = _draftStartProductionQuantities.TryGetValue(task.Id, out var draft)
                ? draft
                : remainingToStart;

            var dialog = new TaskQuantityActionDialog(
                        task: task,
                        actionTitle: "Start Production",
                        actionMessage: "Enter the quantity of items that is starting production:",
                        isCompleteAction: false,
                        maxAllowedQuantity: remainingToStart,
                        initialValue: initialValue)
            {
                XamlRoot = _dialogService.CurrentXamlRoot,
            };

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                int qtyToSubmit = (int)dialog.InputValue;
                _draftStartProductionQuantities[task.Id] = qtyToSubmit;

                try
                {
                    IsBusy = true;

                    await _api.StartProduction(new StartProductionCommand(task.Id, qtyToSubmit));

                    await LoadDataAsync();

                    _toastService.ShowSuccess("Success", $"Marked {qtyToSubmit} items as being in production.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
        private async Task IssuePoAsync(FulfillmentTaskGridItem? task)
        {
            if (task == null || task.Type == FulfillmentTaskType.Production)
                return;

            int remainingToStart = task.RequestedQuantity - task.StartedQuantity;
            if (remainingToStart <= 0)
                return;

            int initialValue = _draftIssuePoQuantities.TryGetValue(task.Id, out var draft)
                ? draft
                : remainingToStart;

            string initialPoNumber = _draftIssuePoNumbers.TryGetValue(task.Id, out var draftPoNumber)
                ? draftPoNumber
                : "";

            double initialCostAmount = _draftIssuePoCosts.TryGetValue(task.Id, out var draftCostAmount)
                ? draftCostAmount
                : 0;

            var dialog = new IssuePoDialog(
                        task: task,
                        initialQuantity: initialValue,
                        initialPoNumber: initialPoNumber,
                        initialCost: initialCostAmount,
                        baseCurrency: _tenantProfile.Profile?.BaseCurrency ?? "LKR")
            {
                XamlRoot = _dialogService.CurrentXamlRoot,
            };

            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                int qtyToSubmit = (int)dialog.IssueQuantity;
                _draftIssuePoCosts[task.Id] = qtyToSubmit;

                string poNumberToSumit = (string)dialog.PoNumber;
                _draftIssuePoNumbers[task.Id] = poNumberToSumit;

                decimal costToSubmit = (decimal)dialog.CostAmount;
                _draftIssuePoCosts[task.Id] = (double)costToSubmit;

                try
                {
                    IsBusy = true;

                    await _api.IssuePurchaseOrder(new IssuePurchaseOrderCommand(
                        task.Id, 
                        qtyToSubmit, 
                        poNumberToSumit, 
                        new MoneyDto(costToSubmit, _tenantProfile.Profile?.BaseCurrency ?? "LKR")));

                    await LoadDataAsync();

                    _toastService.ShowSuccess("Success", $"Marked {qtyToSubmit} items as being in production.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
        private async Task MarkReadyAsync(FulfillmentTaskGridItem? task)
        {
            if (task == null)
                return;

            int remainingToComplete = task.RequestedQuantity - (task.CompletedQuantity + task.ScrappedQuantity);
            if (remainingToComplete <= 0)
                return;

            int initialValue = _draftMarkReadyQuantities.TryGetValue(task.Id, out var draft)
                ? draft
                : remainingToComplete;

            var dialog = new TaskQuantityActionDialog(
                        task: task,
                        actionTitle: "Mark Items Ready",
                        actionMessage: "Enter the quantity of items that have been completed:",
                        isCompleteAction: true,
                        maxAllowedQuantity: remainingToComplete,
                        initialValue: initialValue)
            {
                XamlRoot = _dialogService.CurrentXamlRoot,
            };

            var result = await dialog.ShowAsync();

            if(result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                int qtyToSubmit = (int)dialog.InputValue;
                bool? allocateToOrder = dialog.AllocateToOrder;
                _draftMarkReadyQuantities[task.Id] = qtyToSubmit;

                try
                {
                    IsBusy = true;

                    if (task.Type == FulfillmentTaskType.Production)
                        await _api.CompleteProduction(new CompleteProductionTaskCommand(task.Id, qtyToSubmit, allocateToOrder));
                    else if (task.Type == FulfillmentTaskType.Procurement)
                        await _api.CompleteProcurement(new CompleteProcurementTaskCommand(task.Id, qtyToSubmit, allocateToOrder));

                    await LoadDataAsync();

                    _toastService.ShowSuccess("Success", $"Marked {qtyToSubmit} items ready.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
                }
                finally
                {
                    IsBusy= false;
                }
            }
        }

        private async Task CompleteBatchAsync(FulfillmentGroup? group)
        {
            if (group == null) return;
            int remaining = group.TotalRequested - group.TotalCompleted;
            if (remaining <= 0)
            {
                _toastService.ShowWarning("Notice", "All tasks in this batch are already completed.");
                return;
            }
            var dialog = new CompleteBatchDialog(group.ProductName, group.Sku, remaining)
            {
                XamlRoot = _dialogService.CurrentXamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                try
                {
                    IsBusy = true;

                    await _api.CompleteBatch(new CompleteBatchCommand(group.ProductVariantId, dialog.AllocateToOrders));

                    _toastService.ShowSuccess("Success", $"Batch for {group.ProductName} completed.");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error completing batch: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task CancelTaskAsync(FulfillmentTaskGridItem? task)
        {
            if (IsBusy || task == null)
                return;

            int inProgressQuantity = task.StartedQuantity - (task.CompletedQuantity + task.ScrappedQuantity);
            int unstartedQuantity = task.RequestedQuantity - task.StartedQuantity;

            var messageBuilder = new System.Text.StringBuilder();
            messageBuilder.AppendLine($"Are you sure you want to cancel the '{task.ProductName}' {task.Type} task?");
            messageBuilder.AppendLine(); // Empty line for spacing

            if (task.CompletedQuantity > 0)
            {
                messageBuilder.AppendLine($"• {task.CompletedQuantity} completed items will remain safely in Stock on Hand.");
            }

            if (inProgressQuantity > 0)
            {
                messageBuilder.AppendLine($"• {inProgressQuantity} in-progress items will be halted and removed from Incoming Stock.");
            }

            if (unstartedQuantity > 0)
            {
                messageBuilder.AppendLine($"• {unstartedQuantity} pending items will be discarded.");
            }

            messageBuilder.AppendLine();
            messageBuilder.Append("This action cannot be undone.");

            var confirmed = await _dialogService.ShowConfirmationAsync(
                $"Cancel {task.Type} Task",
                messageBuilder.ToString(),
                "Yes, Cancel Task",
                "Go Back");

            if (confirmed)
            {
                try
                {
                    IsBusy = true;
                    if (task.Type == FulfillmentTaskType.Production)
                        await _api.CancelProduction(new CancelProductionTaskCommand(task.Id));
                    else if (task.Type == FulfillmentTaskType.Procurement)
                        await _api.CancelProcurementn(new CancelProcurementTaskCommand(task.Id));

                    _toastService.ShowSuccess("Success", "Task cancelled successfully.");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
