using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Reports
{
    public partial class FinancialReportViewModel : ObservableObject, INavigationAware
    {
        private readonly IReportsApi _reportsApi;
        private readonly ILogger<FinancialReportViewModel> _logger;

        // ── Inputs ───────────────────────────────────────────
        public ObservableCollection<int> AvailableYears { get; } = new();
        public ObservableCollection<MonthItem> AvailableMonths { get; } = new();

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value);
        }

        private MonthItem? _selectedMonth;
        public MonthItem? SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value);
        }

        // ── Report Data ───────────────────────────────────────
        private MonthlyFinancialReportDto? _report;
        public MonthlyFinancialReportDto? Report
        {
            get => _report;
            set
            {
                if (SetProperty(ref _report, value))
                {
                    OnPropertyChanged(nameof(HasReport));
                    OnPropertyChanged(nameof(ReportTitle));
                    OnPropertyChanged(nameof(FormattedGrossSales));
                    OnPropertyChanged(nameof(FormattedNetRevenue));
                    OnPropertyChanged(nameof(FormattedGrossProfit));
                    OnPropertyChanged(nameof(FormattedNetProfit));
                    OnPropertyChanged(nameof(FormattedCogs));
                    OnPropertyChanged(nameof(FormattedTotalDiscounts));
                    OnPropertyChanged(nameof(FormattedShippingRevenue));
                    OnPropertyChanged(nameof(FormattedTotalExpenses));
                    OnPropertyChanged(nameof(GrossMarginLabel));
                    OnPropertyChanged(nameof(NetMarginLabel));
                    OnPropertyChanged(nameof(DiscountLabel));
                    OnPropertyChanged(nameof(OrderCountLabel));
                    OnPropertyChanged(nameof(NetProfitBrush));
                    RebuildExpenseItems();
                }
            }
        }
        public bool HasReport => _report != null;

        // Expense UI items
        public ObservableCollection<ExpenseCategoryUIItem> ExpenseItems { get; } = new();

        // ── Loading ───────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // ── Commands ──────────────────────────────────────────
        public IAsyncRelayCommand LoadReportCommand { get; }

        public FinancialReportViewModel(IReportsApi reportsApi, ILogger<FinancialReportViewModel> logger)
        {
            _reportsApi = reportsApi;
            _logger = logger;

            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);

            // Populate year picker: last 3 years + current
            var currentYear = DateTime.Now.Year;
            for (int y = currentYear; y >= currentYear - 3; y--)
                AvailableYears.Add(y);
            // Populate month picker
            for (int m = 1; m <= 12; m++)
                AvailableMonths.Add(new MonthItem(m, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)));
            // Default to current month
            SelectedYear = currentYear;
            SelectedMonth = AvailableMonths.FirstOrDefault(x => x.Number == DateTime.Now.Month);
        }

        private async Task LoadReportAsync()
        {
            if (SelectedMonth is null) return;

            try
            {
                IsLoading = true;
                Report = await _reportsApi.GetMonthlyFinancialReportAsync(SelectedYear, SelectedMonth.Number);

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

        private void RebuildExpenseItems()
        {
            ExpenseItems.Clear();
            if (_report is null) return;
            foreach (var cat in _report.ExpensesByCategory)
                ExpenseItems.Add(new ExpenseCategoryUIItem(cat, _report.TatalExpenses));
        }

        // ── Formatted Strings ─────────────────────────────────
        public string ReportTitle => _report is null
            ? "—"
            : $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_report.Month)} {_report.Year}";
        private string Fmt(decimal amount) =>
            _report is null ? "—" : $"{_report.Currency} {amount:N2}";
        public string FormattedGrossSales => Fmt(_report?.GrossSales ?? 0);
        public string FormattedNetRevenue => Fmt(_report?.NetRevenue ?? 0);
        public string FormattedGrossProfit => Fmt(_report?.GrossProfit ?? 0);
        public string FormattedNetProfit => Fmt(_report?.NetProfit ?? 0);
        public string FormattedCogs => Fmt(_report?.CostofGoodsSold ?? 0);
        public string FormattedTotalDiscounts => Fmt(_report?.TotalDiscounts ?? 0);
        public string FormattedShippingRevenue => Fmt(_report?.ShippingRevenue ?? 0);
        public string FormattedTotalExpenses => Fmt(_report?.TatalExpenses ?? 0);
        public string GrossMarginLabel => _report is null ? "—"
            : $"{_report.GrossMarginPercent:F1}% gross margin";
        public string NetMarginLabel => _report is null ? "—"
            : $"{_report.NetMarginPercent:F1}% net margin";
        public string DiscountLabel => _report is null ? "—"
            : $"− {_report.Currency} {_report.TotalDiscounts:N2} discounts";
        public string OrderCountLabel => _report is null ? "—"
            : $"{_report.TotalCompeltedOrders} completed orders";

        // Green if profit, red if loss
        public SolidColorBrush NetProfitBrush => (_report?.NetProfit ?? 0) >= 0
            ? new SolidColorBrush(Colors.Green)   // or use ThemeResource via code
            : new SolidColorBrush(Colors.Crimson);

        public void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadReportAsync();
        }
    }

    // ── Supporting types ───────────────────────────────────────────
    public record MonthItem(int Number, string Label);


    public class ExpenseCategoryUIItem
    {
        public string Category { get; }
        public string FormattedAmount { get; }
        public double Maximum { get; }
        public double Value { get; }
        public ExpenseCategoryUIItem(ExpenseCategorySummaryDto dto, decimal totalExpenses)
        {
            Category = dto.Category;
            FormattedAmount = $"{dto.TotalAmount:N2}";
            Maximum = totalExpenses > 0 ? (double)totalExpenses : 1.0;
            Value = (double)dto.TotalAmount;
        }
    }
}
