using System;

namespace Onyx.Oms.Client.Desktop.Features.Expenses.List
{
    public class ExpenseGridItem
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTimeOffset DateIncurred { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }

        public string FormattedAmount {  get; set; } = string.Empty;
        public string DateIncurredDisplay => DateIncurred.ToLocalTime().ToString("d");

        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        public ExpenseGridItem(ExpenseDto expenseDto, bool canEdit, bool canDelete)
        {
            Id = expenseDto.Id;
            Category = expenseDto.Category;
            Amount = expenseDto.Amount;
            Currency = expenseDto.Currency;
            DateIncurred = expenseDto.DateIncurred;
            Reference = expenseDto.Reference;
            Notes = expenseDto.Notes;
            CreatedOnUtc = expenseDto.CreatedOnUtc;

            FormattedAmount = $"{Currency} {Amount:N2}";

            CanEdit = canEdit; 
            CanDelete = canDelete;
        }
    }
}
