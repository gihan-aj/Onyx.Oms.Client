namespace Onyx.Oms.Client.Desktop.Features.Orders;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    ReadyToPack,
    Packed,
    Shipped,
    Delivered,
    Completed,
    PaymentFailed,
    Cancelled,
    ReturnedToSender,
    DeliveryFailed
}
