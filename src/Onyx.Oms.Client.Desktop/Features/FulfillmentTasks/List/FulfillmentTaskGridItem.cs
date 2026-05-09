namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public class FulfillmentTaskGridItem : FulfillmentTaskDto
    {
        public string ExpectedCompletionDateDisplay => ExpectedCompletionDate?.ToString("MMM dd, yyyy") ?? "-";
        public bool IsProductionPending => Type == FulfillmentTaskType.Production && RequestedQuantity > StartedQuantity;
        public bool IsProcurementPending => Type == FulfillmentTaskType.Procurement && RequestedQuantity > StartedQuantity;
        public bool IsInProgress => Status == FulfillmentTaskStatus.InProgress || Status == FulfillmentTaskStatus.Pending;
        public bool CanBeCancelled => Status == FulfillmentTaskStatus.Pending || Status == FulfillmentTaskStatus.InProgress;
        public bool CanEdit { get; set; }
        public string AssigneeName => Type == FulfillmentTaskType.Procurement ? "N/A" 
            : (string.IsNullOrEmpty(AssignedUserFirstName) ? "Unassigned" : $"{AssignedUserFirstName} {AssignedUserLastName}");
        public bool HasScrapped => ScrappedQuantity > 0;
    }

    public static class FulfillmentTaskMappingExtensions
    {
        public static FulfillmentTaskGridItem ToGridItem(this FulfillmentTaskDto dto, bool canEdit)
        {
            var gridItem = new FulfillmentTaskGridItem
            {
                Id = dto.Id,
                Type = dto.Type,
                ProductVariantId = dto.ProductVariantId,
                ProductName = dto.ProductName,
                Sku = dto.Sku,
                ProductHasVariants = dto.ProductHasVariants,
                VariantAttributes = dto.VariantAttributes,
                RequestedQuantity = dto.RequestedQuantity,
                LinkedOrderItemId = dto.LinkedOrderItemId,
                OrderNumber = dto.OrderNumber,
                Cost = dto.Cost,
                AssignedUserId = dto.AssignedUserId,
                AssignedUserFirstName = dto.AssignedUserFirstName,
                AssignedUserLastName = dto.AssignedUserLastName,
                PurchaseOrderNumber = dto.PurchaseOrderNumber,
                Notes = dto.Notes,
                ExpectedCompletionDate = dto.ExpectedCompletionDate,
                Priority = dto.Priority,
                Status = dto.Status,
                CreatedOnUtc = dto.CreatedOnUtc,
                StartedQuantity = dto.StartedQuantity,
                CompletedQuantity = dto.CompletedQuantity,
                ScrappedQuantity = dto.ScrappedQuantity,
                CanEdit = canEdit
            };

            return gridItem;
        }
    }
}
