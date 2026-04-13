namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public class FulfillmentTaskGridItem : FulfillmentTaskDto
    {
        public bool CanEdit { get; set; }
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
                CanEdit = canEdit
            };

            return gridItem;
        }
    }
}
