using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.Create;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public partial class FulfillmentTasksViewModel : PagedDataGridViewModelBase<FulfillmentTaskGridItem>, INavigationAware
    {
        private readonly IFulfillmentTasksApi _api;
        private readonly IPermissionService _permissionService;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly IDialogService _dialogService;

        // Drafts
        private readonly Dictionary<Guid, int> _draftMarkReadyQuantities = new();

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
            set => SetProperty(ref _selectedType, value);
        }

        private TaskPriority? _selectedPriority;
        public TaskPriority? SelectedPriority
        {
            get => _selectedPriority;
            set => SetProperty(ref _selectedPriority, value);
        }

        private DateTimeOffset? _selectedDate;
        public DateTimeOffset? SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
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
        public IAsyncRelayCommand StartWorkCommand { get; }
        public IAsyncRelayCommand IssuePoCommand { get; }
        public IAsyncRelayCommand MarkReadyCommand { get; }
        public IAsyncRelayCommand CancelTaskCommand { get; }

        public FulfillmentTasksViewModel(IFulfillmentTasksApi api, IPermissionService permissionService, INavigationService navigationService, IToastService toastService, IDialogService dialogService)
        {
            _api = api;
            _permissionService = permissionService;
            _navigationService = navigationService;
            _toastService = toastService;
            _dialogService = dialogService;

            ClearFiltersCommand = new AsyncRelayCommand(ClearFlitersAsync);
            NewTaskCommand = new RelayCommand(NavigateToNewTask);
            StartWorkCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(StartWorkAsync);
            IssuePoCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(IssuePoAsync);
            MarkReadyCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(MarkReadyAsync);
            CancelTaskCommand = new AsyncRelayCommand<FulfillmentTaskGridItem>(CancelTaskAsync);
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
                    expectedCompletionDate: SelectedDate);

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
                    g.Sum(t => t.RequestedQuantity),
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
            Page = 1;
            await LoadDataAsync();
        }

        private void NavigateToNewTask()
        {
            _navigationService.NavigateTo(typeof(CreateFulfillmentTaskPage).FullName!);
        }

        private async Task StartWorkAsync(FulfillmentTaskGridItem? task)
        {

        }
        private async Task IssuePoAsync(FulfillmentTaskGridItem? task)
        {

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
                        maxAllowedQuantity: remainingToComplete,
                        initialValue: initialValue)
            {
                XamlRoot = _dialogService.CurrentXamlRoot,
            };

            var result = await dialog.ShowAsync();

            if(result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                int qtyToSubmit = (int)dialog.InputValue;
                _draftMarkReadyQuantities[task.Id] = qtyToSubmit;

                try
                {
                    IsBusy = true;

                    if (task.Type == FulfillmentTaskType.Production)
                        await _api.CompleteProduction(new CompleteProductionTaskCommand(task.Id, qtyToSubmit));
                    else if (task.Type == FulfillmentTaskType.Procurement)
                        await _api.CompleteProcurement(new CompleteProcurementTaskCommand(task.Id, qtyToSubmit));

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
        private async Task CancelTaskAsync(FulfillmentTaskGridItem? task)
        {

        }
    }
}
