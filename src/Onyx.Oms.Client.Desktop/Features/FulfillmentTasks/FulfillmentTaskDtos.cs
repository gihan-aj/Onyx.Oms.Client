using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks
{
    public class FulfillmentTaskDto
    {
        public Guid Id { get; set; }
        public FulfillmentTaskType Type { get; set; }
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public bool ProductHasVariants { get; set; }
        public List<VariantAttributeDto>? VariantAttributes { get; set; }
        public int RequestedQuantity { get; set; }
        public Guid? LinkedOrderItemId { get; set; }
        public string? OrderNumber { get; set; }
        public MoneyDto? Cost { get; set; }
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUserFirstName { get; set; }
        public string? AssignedUserLastName { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? ExpectedCompletionDate { get; set; }
        public TaskPriority Priority { get; set; }
        public FulfillmentTaskStatus Status { get; set; }
        public DateTimeOffset CreatedOnUtc {  get; set; }
        public int StartedQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public int ScrappedQuantity { get; set; }
    }

    public record VariantAttributeDto(
        string Name,
        string Value
    );

    public record MoneyDto(decimal Amount, string Currency = "LKR");

    public record CreateProcurementTaskCommand(
        Guid ProductVariantId,
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority);

    public record CreateProductionTaskCommand(
        Guid ProductVariantId,
        int RequestedQuantity,
        Guid? AssignedUserId,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority);

    public record CancelProcurementTaskCommand(Guid ProcurementTaskId);

    public record CancelProductionTaskCommand(Guid ProductionTaskId);

    public record CompleteProcurementTaskCommand(Guid ProcurementTaskId, int QuantityToComplete, bool? allocateToOrder = null);

    public record CompleteProductionTaskCommand(Guid ProductionTaskId, int QuantityToComplete, bool? allocateToOrder = null);

    public record IssuePurchaseOrderCommand(
        Guid ProcurementTaskId,
        int IssueQuantity,
        string PurchaseOrderNumber,
        MoneyDto Cost);

    public record ScrapProcurementTaskCommand(Guid ProcurementTaskId, int QuantityToScrap);

    public record ScrapProductionTaskCommand(Guid ProductionTaskId, int QuantityToScrap);

    public record StartProductionCommand(
        Guid ProductionsTaskId,
        int QuantityToStart);

    public record UpdateProductionTaskCommand(
        Guid ProductionTaskId,
        int RequestedQuantity,
        Guid? AssignedUserId,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority,
        string? Notes);

    public record UpdateProcurementTaskCommand(
        Guid ProcurementTaskId,
        int RequestedQuantity,
        string? PurchaseOrderNumber,
        MoneyDto? Cost,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority,
        string? Notes);

    public record UserDto(
        Guid Id,
        string FirstName,
        string LastName,
        string[] Roles);

    public enum FulfillmentTaskType
    {
        Production = 0,
        Procurement = 1
    }

    public enum FulfillmentTaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Ready = 2,
        Cancelled = 3
    }

    public enum TaskPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Urgent = 3
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseSku { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryPath { get; set; } = string.Empty;
        public decimal BasePriceAmount { get; set; }
        public string BasePriceCurrency { get; set; } = "LKR";
        public string? MainImageUrl { get; set; }
        public bool HasVariants { get; set; }
        public List<ProductOptionDto> Options { get; set; } = new();
        public List<ProductVariantDto>? Variants { get; set; }
        public List<ProductImageDto>? Images { get; set; }
        public int StockOnHand { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public DateTimeOffset? LastModifiedOnUtc { get; set; }
    }

    public record ProductOptionDto(
        string Name,
        int DispalyOrder,
        List<string> Values);

    public record ProductVariantDto(
        Guid Id,
        string Sku,
        List<VariantAttributeDto> Attributes,
        decimal CostAmount,
        string CostCurrency,
        decimal PriceAmount,
        string PriceCurrency,
        decimal? WeightAmount,
        string? WeightUnit,
        int StockOnHand,
        int ReservedQuantity,
        bool IsActive
    );

    public record ProductImageDto(
        Guid Id,
        string Url,
        int DisplayOrder,
        bool IsMain,
        string? OptionName = null,
        string? OptionValue = null
    );

    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int Level { get; set; }
        public string Path { get; set; } = string.Empty;
        public string NamePath { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
