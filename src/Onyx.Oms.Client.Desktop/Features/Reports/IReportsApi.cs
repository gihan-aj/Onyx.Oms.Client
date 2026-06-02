using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Reports
{
    public interface IReportsApi
    {
        [Get("/api/v1/reports/monthly-financials")]
        Task<MonthlyFinancialReportDto> GetMonthlyFinancialReportAsync([Query] int  year, [Query] int month);
    }

    public record MonthlyFinancialReportDto(
        string Currency,
        int Year,
        int Month,
        int TotalCompeltedOrders,

        // REVENUE
        decimal GrossSales,         // Sum of all items at selling price
        decimal TotalDiscounts,     // Sum of all discounts
        decimal ShippingRevenue,    // Sum of shipping fees charged to customers
        decimal NetRevenue,         // (Gross Sales + Shipping) - Discounts

        // COST OF GOODS (COGS)
        decimal CostofGoodsSold,    // Sum of (Qty * Unit Cost)

        // GROSS PROFIT
        decimal GrossProfit,        // NetRevenue - COGS
        decimal GrossMarginPercent, // (GrossProfit / NetRevenue) * 100

        // OPERATING EXPENSES
        decimal TatalExpenses,      // Sum of all expenses
        List<ExpenseCategorySummaryDto> ExpensesByCategory,

        // NET PROFIT
        decimal NetProfit,          // Gross Profit - TotalExpenses
        decimal NetMarginPercent    // (NetProfit / NetRevenue) * 100
    );

    public record ExpenseCategorySummaryDto(string Category, decimal TotalAmount);
}
