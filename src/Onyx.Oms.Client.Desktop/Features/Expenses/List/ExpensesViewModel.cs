using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.Customers;
using Onyx.Oms.Client.Desktop.Features.Expenses.Form;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using static Onyx.Oms.Client.Desktop.Shared.Constants.Permissions;

namespace Onyx.Oms.Client.Desktop.Features.Expenses.List
{
    public partial class ExpensesViewModel : PagedDataGridViewModelBase<ExpenseGridItem>, INavigationAware
    {
        private readonly IExpensesApi _expensesApi;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<ExpensesViewModel> _logger;
        private readonly IToastService _toastService;
        private readonly INavigationService _navigationService;
        private readonly ITenantProfileService _tenantProfileService;
        private readonly IDialogService _dialogService;

        public string BaseCurrency { get; private set; } = string.Empty;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        private DateTimeOffset? _fromDate;
        public DateTimeOffset? FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        private DateTimeOffset? _toDate;
        public DateTimeOffset? ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        private string? _selectedCategory;
        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        private decimal? _minAmount;
        public decimal? MinAmount
        {
            get => _minAmount;
            set
            {
                if (SetProperty(ref _minAmount, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        private decimal? _maxAmount;
        public decimal? MaxAmount
        {
            get => _maxAmount;
            set
            {
                if (SetProperty(ref _maxAmount, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public bool CanCreate {  get; }
        public bool CanEdit {  get; }
        public bool CanDelete {  get; }

        public IAsyncRelayCommand ClearFiltersCommand { get; }
        public IRelayCommand NewExpenseCommand { get; }
        public IRelayCommand<ExpenseGridItem> EditExpenseCommand { get; }
        public IAsyncRelayCommand<ExpenseGridItem> DeleteExpenseCommand { get; }

        public ExpensesViewModel(
            IExpensesApi expensesApi,
            IPermissionService permissionService,
            ILogger<ExpensesViewModel> logger,
            IToastService toastService,
            INavigationService navigationService,
            ITenantProfileService tenantProfileService,
            IDialogService dialogService)
        {
            _expensesApi = expensesApi;
            _permissionService = permissionService;
            _logger = logger;
            _toastService = toastService;
            _navigationService = navigationService;
            _tenantProfileService = tenantProfileService;
            _dialogService = dialogService;

            CanCreate = _permissionService.CanExecute(Permissions.Expenses.Create);
            CanEdit = _permissionService.CanExecute(Permissions.Expenses.Edit);
            CanDelete = _permissionService.CanExecute(Permissions.Expenses.Delete);

            BaseCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
            _fromDate = DateTimeOffset.Now.AddDays(-30);
            _toDate = DateTimeOffset.Now;

            ClearFiltersCommand = new AsyncRelayCommand(OnClearFiltersAsync);
            NewExpenseCommand = new RelayCommand(OnNewExpense);
            EditExpenseCommand = new RelayCommand<ExpenseGridItem>(OnEditExpense);
            DeleteExpenseCommand = new AsyncRelayCommand<ExpenseGridItem>(OnDeleteExpenseAsync);
        }

        protected async override Task LoadDataAsync()
        {
            if (IsListLoading)
                return;

            try
            {
                IsListLoading = true;
                var pagedResult = await _expensesApi.GetExpensesPagedAsync(
                    Page,
                    PageSize,
                    SearchTerm,
                    SortColumn,
                    SortOrder,
                    FromDate,
                    ToDate,
                    SelectedCategory,
                    MinAmount,
                    MaxAmount);

                Items.Clear();
                foreach (var expense in pagedResult.Items)
                {
                    Items.Add(new ExpenseGridItem(expense, CanEdit, CanDelete));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load expenses");
            }
            finally
            {
                IsListLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _expensesApi.GetExpenseCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load expense categories");
            }
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadCategoriesAsync();
            await LoadDataAsync();
        }

        public void OnNavigatedFrom()
        {
            
        }

        private async Task OnClearFiltersAsync()
        {
            SearchTerm = string.Empty;
            FromDate = DateTimeOffset.Now.AddDays(-30);
            ToDate = DateTimeOffset.Now;
            SelectedCategory = null;
            MinAmount = null;
            MaxAmount = null;
            await LoadDataAsync();
        }

        private void OnNewExpense()
        {
            if (CanCreate)
                _navigationService.NavigateTo(typeof(ExpenseFormPage).FullName!);
        }

        private void OnEditExpense(ExpenseGridItem? item)
        {
            if (item == null || !CanEdit)
                return;

            // Reconstruct the ExpenseDto from the grid item so the form can populate itself
            var dto = new ExpenseDto(
                Id: item.Id,
                Category: item.Category,
                Amount: item.Amount,
                Currency: item.Currency,
                DateIncurred: item.DateIncurred,
                Reference: item.Reference,
                Notes: item.Notes,
                CreatedOnUtc: item.CreatedOnUtc);

            _navigationService.NavigateTo(typeof(ExpenseFormPage).FullName!, dto);
        }

        private async Task OnDeleteExpenseAsync(ExpenseGridItem? item)
        {
            if(item == null || !CanDelete) 
                return;

            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Delete Expense",
                $"Are you sure you want to delete this expense?",
                "Delete",
                "Cancel");

            if (confirmed)
            {
                try
                {
                    IsBusy = true;
                    await _expensesApi.DeleteExpenseAsync(item.Id);
                    _toastService.ShowSuccess("Success", "Expense deleted successfully.");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting customer");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
