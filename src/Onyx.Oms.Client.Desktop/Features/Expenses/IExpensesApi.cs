using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Expenses
{
    public interface IExpensesApi
    {
        [Get("/api/v1/expenses")]
        Task<PagedResult<ExpenseDto>> GetExpensesPagedAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortOrder = null,
            DateTimeOffset? dateFrom = null,
            DateTimeOffset? dateTo = null,
            string? category = null,
            decimal? minAmount = null,
            decimal? maxAmount = null);

        [Post("/api/v1/expenses")]
        Task<Guid> CreateExpenseAsync([Body] CreateExpenseRequest request);

        [Put("/api/v1/expenses/{id}")]
        Task UpdateExpenseAsync(Guid id, [Body] UpdateExpenseRequest request);

        [Delete("/api/v1/expenses/{id}")]
        Task DeleteExpenseAsync(Guid id);

        [Get("/api/v1/expenses/categories")]
        Task<IReadOnlyList<string>> GetExpenseCategoriesAsync();
    }

    public record ExpenseDto(
        Guid Id,
        string Category,
        decimal Amount,
        string Currency,
        DateTimeOffset DateIncurred,
        string? Reference,
        string? Notes,
        DateTimeOffset CreatedOnUtc);

    public record CreateExpenseRequest(
        string Category,
        decimal Amount,
        string Currency,
        DateTimeOffset DateIncurred,
        string? Reference,
        string? Notes);

    public record UpdateExpenseRequest(
        string Category,
        decimal Amount,
        string Currency,
        DateTimeOffset DateIncurred,
        string? Reference,
        string? Notes);
}
