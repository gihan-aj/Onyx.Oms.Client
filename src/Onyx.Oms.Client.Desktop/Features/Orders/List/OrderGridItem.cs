namespace Onyx.Oms.Client.Desktop.Features.Orders.List
{
    public class OrderGridItem : OrderSummaryDto
    {
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }

        public string CustomerDisplay => string.IsNullOrWhiteSpace(CustomerEmail)
            ? CustomerName
            : $"{CustomerName} ({CustomerEmail})";

        public string GrandTotalDisplay => $"{GrandTotalCurrency} {GrandTotalAmount:N2}";
        public string BalanceDisplay => $"{GrandTotalCurrency} {BalanceAmount:N2}";

        public string OrderDateDisplay => OrderDate?.ToLocalTime().ToString("g") ?? "-";

        public bool CanConfirm => Status == OrderStatus.Pending;
        public bool CanCancel => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed || Status == OrderStatus.Processing || Status == OrderStatus.ReadyToPack || Status == OrderStatus.Packed;
        public bool CanProgress => Status != OrderStatus.Pending && Status != OrderStatus.Cancelled && Status != OrderStatus.Completed && Status != OrderStatus.Delivered && Status != OrderStatus.PaymentFailed;
    }

    public static class OrderMappingExtensions
    {
        public static OrderGridItem ToGridItem(
            this OrderSummaryDto dto,
            bool canEdit,
            bool canView)
        {
            return new OrderGridItem
            {
                Id = dto.Id,
                OrderNumber = dto.OrderNumber,
                OrderDate = dto.OrderDate,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                PrimaryPhone = dto.PrimaryPhone,
                Status = dto.Status,
                PaymentStatus = dto.PaymentStatus,
                GrandTotalAmount = dto.GrandTotalAmount,
                GrandTotalCurrency = dto.GrandTotalCurrency,
                TotalPaidAmount = dto.TotalPaidAmount,
                BalanceAmount = dto.BalanceAmount,
                IsCashOnDelivery = dto.IsCashOnDelivery,
                TrackingNumber = dto.TrackingNumber,
                CreatedOnUtc = dto.CreatedOnUtc,
                LastModifiedOnUtc = dto.LastModifiedOnUtc,

                CanEdit = canEdit,
                CanView = canView
            };
        }
    }
}
