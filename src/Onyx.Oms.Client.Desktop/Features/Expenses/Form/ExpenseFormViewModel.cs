using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.Expenses.List;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Expenses.Form
{
    public partial class ExpenseFormViewModel : ObservableObject, INavigationAware
    {
        private readonly IExpensesApi _expensesApi;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly ILogger<ExpenseFormViewModel> _logger;
        private readonly ITenantProfileService _tenantProfile;

        // ── State ─────────────────────────────────────────────────────────────

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            private set => SetProperty(ref _isEditMode, value);
        }

        private Guid? _expenseId;
        public Guid? ExpenseId
        {
            get => _expenseId;
            private set => SetProperty(ref _expenseId, value);
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

        private string _pageTitle = string.Empty;
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        // ── Form fields ───────────────────────────────────────────────────────

        private string? _selectedCategory;
        public string? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<string> Categories { get; } = new();

        private string _currency = "LKR";
        public string Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _amountHeader = "Amount";
        public string AmountHeader
        {
            get => _amountHeader;
            set => SetProperty(ref _amountHeader, value);
        }

        private DateTimeOffset? _dateIncurred = DateTimeOffset.Now;
        public DateTimeOffset? DateIncurred
        {
            get => _dateIncurred;
            set => SetProperty(ref _dateIncurred, value);
        }

        private string _reference = string.Empty;
        public string Reference
        {
            get => _reference;
            set => SetProperty(ref _reference, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // ── Validation errors ─────────────────────────────────────────────────

        private string? _categoryError;
        public string? CategoryError
        {
            get => _categoryError;
            set => SetProperty(ref _categoryError, value);
        }

        private string? _amountError;
        public string? AmountError
        {
            get => _amountError;
            set => SetProperty(ref _amountError, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public IRelayCommand GoBackCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        public ExpenseFormViewModel(
            IExpensesApi expensesApi,
            INavigationService navigationService,
            IToastService toastService,
            ILogger<ExpenseFormViewModel> logger,
            ITenantProfileService tenantProfile)
        {
            _expensesApi = expensesApi;
            _navigationService = navigationService;
            _toastService = toastService;
            _logger = logger;
            _tenantProfile = tenantProfile;

            Currency = _tenantProfile.Profile?.BaseCurrency ?? "LKR";
            AmountHeader = $"Amount ({Currency})";

            GoBackCommand = new RelayCommand(OnGoBack);
            SaveCommand = new AsyncRelayCommand(OnSaveAsync);
        }

        // ── INavigationAware ──────────────────────────────────────────────────

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is ExpenseDto dto)
                await InitializeAsync(dto);
            else
                await InitializeAsync();
        }

        public void OnNavigatedFrom() { }

        // ── Initialization ────────────────────────────────────────────────────

        private async Task InitializeAsync(ExpenseDto? expense = null)
        {
            IsLoading = true;
            try
            {
                await LoadCategoriesAsync();

                if (expense != null)
                {
                    IsEditMode = true;
                    ExpenseId = expense.Id;
                    PageTitle = $"Edit {expense.Category} Expense";

                    SelectedCategory = expense.Category;
                    Amount = expense.Amount;
                    Currency = expense.Currency;
                    AmountHeader = $"Amount ({Currency})";
                    DateIncurred = (DateTimeOffset?)expense.DateIncurred;
                    Reference = expense.Reference ?? string.Empty;
                    Notes = expense.Notes ?? string.Empty;
                }
                else
                {
                    IsEditMode = false;
                    ExpenseId = null;
                    PageTitle = "Create Expense";

                    SelectedCategory = null;
                    Amount = 0;
                    Currency = _tenantProfile.Profile?.BaseCurrency ?? "LKR";
                    AmountHeader = $"Amount ({Currency})";
                    DateIncurred = DateTimeOffset.Now;
                    Reference = string.Empty;
                    Notes = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize expense form");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _expensesApi.GetExpenseCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                    Categories.Add(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load expense categories");
            }
        }

        // ── Commands implementation ───────────────────────────────────────────

        private void OnGoBack()
        {
            if (_navigationService.CanGoBack)
                _navigationService.GoBack();
            else
                _navigationService.NavigateTo(typeof(ExpensesPage).FullName!);
        }

        /// <summary>
        /// Adds a custom category string to the local Categories list and selects it.
        /// No API call is made — the value is sent to the server on Save.
        /// </summary>
        public void AddCustomCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return;

            var trimmed = category.Trim();

            // Avoid duplicates (case-insensitive)
            if (!Categories.Any(c => string.Equals(c, trimmed, StringComparison.OrdinalIgnoreCase)))
                Categories.Add(trimmed);

            SelectedCategory = trimmed;
            CategoryError = null;
        }

        private async Task OnSaveAsync()
        {
            var saved = await SaveAsync();
            if (saved)
                OnGoBack();
        }

        public async Task<bool> SaveAsync()
        {
            IsBusy = true;
            CategoryError = null;
            AmountError = null;

            try
            {
                if (IsEditMode)
                {
                    var updateRequest = new UpdateExpenseRequest(
                        Category: SelectedCategory ?? string.Empty,
                        Amount: Amount,
                        Currency: Currency,
                        DateIncurred: DateIncurred ?? DateTimeOffset.Now,
                        Reference: string.IsNullOrWhiteSpace(Reference) ? null : Reference,
                        Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes);

                    await _expensesApi.UpdateExpenseAsync(ExpenseId!.Value, updateRequest);
                    _toastService.ShowSuccess("Success", "Expense updated successfully.");
                }
                else
                {
                    var createRequest = new CreateExpenseRequest(
                        Category: SelectedCategory ?? string.Empty,
                        Amount: Amount,
                        Currency: Currency,
                        DateIncurred: DateIncurred ?? DateTimeOffset.Now,
                        Reference: string.IsNullOrWhiteSpace(Reference) ? null : Reference,
                        Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes);

                    await _expensesApi.CreateExpenseAsync(createRequest);
                    _toastService.ShowSuccess("Success", "Expense created successfully.");
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
                        if (string.Equals(error.Code, "Category", StringComparison.OrdinalIgnoreCase) ||
                            error.Description?.Contains("Category", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            CategoryError = error.Description;
                        }
                        else if (string.Equals(error.Code, "Amount", StringComparison.OrdinalIgnoreCase) ||
                                 error.Description?.Contains("Amount", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            AmountError = error.Description;
                        }
                    }
                }
                else
                {
                    _toastService.ShowError("Error", problemDetails?.Detail ?? "Failed to save expense.");
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save expense");
                //_toastService.ShowError("Error", "An unexpected error occurred while saving.");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
