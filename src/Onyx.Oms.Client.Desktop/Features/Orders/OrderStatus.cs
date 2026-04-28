namespace Onyx.Oms.Client.Desktop.Features.Orders;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    ReadyToPack = 3,
    Packed = 4,
    Shipped = 5,
    Delivered = 6,
    Completed = 7,
    PaymentFailed = 8,
    Cancelled = 9,
    ReturnedToSender = 10,
    DeliveryFailed = 11
}

public enum OrderItemStatus
{
    Allocated = 0,
    Pending = 1,
    InProduction = 2,
    Ordered = 3, // Procurement
    Ready = 4
}

public enum DiscountType
{
    FlatAmount = 0,
    Percentage = 1
}

public enum PaymentMethod
{
    CashOnDelivery = 0,
    BankTransfer = 1,
    Card = 2
}

public enum PaymentStatus
{
    Unpaid = 0,
    PartiallyPaid = 1,
    FullyPaid = 2
}

public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}