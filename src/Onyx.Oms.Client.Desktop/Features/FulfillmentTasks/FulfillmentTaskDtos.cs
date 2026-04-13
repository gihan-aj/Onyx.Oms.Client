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
    }

    public record VariantAttributeDto(
        string Name,
        string Value
    );

    public record MoneyDto(decimal Amount, string Currency = "LKR");

    public record CreateProcurementTaskCommand(
        Guid ProductVariantId,
        int RequestedQuantity,
        MoneyDto Cost,
        string PurchaseOrderNumber,
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
}
