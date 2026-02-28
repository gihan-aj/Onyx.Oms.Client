using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Shared.Models;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BaseSku { get; init; } = string.Empty;
    public string? CategoryName { get; init; }
    public string? Brand { get; init; }
    public bool IsActive { get; init; }
    public bool HasColor { get; init; }
    public bool HasSize { get; init; }
    public decimal BasePriceAmount { get; init; }
    public string BasePriceCurrency { get; init; } = string.Empty;
    public decimal BaseCostAmount { get; init; }
    public string BaseCostCurrency { get; init; } = string.Empty;
    public int TotalStock { get; init; }
    public DateTime CreatedOnUtc { get; init; }

    // UI Helper properties that are hydrated post-fetch
    public bool CanEdit { get; set; }
    public bool CanActivate { get; set; }
    public bool CanDeactivate { get; set; }
}
