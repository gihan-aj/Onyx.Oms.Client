using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.Orders
{
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTimeOffset? OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string PrimaryPhone { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public string GrandTotalCurrency {  get; set; } = "LKR";
        public decimal TotalPaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public string? TrackingNumber { get; set; }
        public List<OrderItemSummaryDto>? Items { get; set; }
        public List<OrderPaymentSummaryDto>? Payments { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public DateTimeOffset? LastModifiedOnUtc { get; set; }
    }

    public class OrderItemSummaryDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceAmount { get; set; }
        public string UnitPriceCurrency { get; set; } = "LKR";
        public decimal LineTotalAmount { get; set; }
        public OrderItemStatus Status { get; set; }
    }

    public class OrderPaymentSummaryDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "LKR";
        public PaymentMethod Method { get; set; }
        public string? Reference { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
    }

    public record OrderStatusCountDto
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
    }

    public record GetOrderStatusCountsResponse
    {
        public List<OrderStatusCountDto> Counts { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public record BulkGenerateShippingLabelsCommand(List<Guid> OrderIds);

    public record CreateOrderCommand(
        Guid CustomerId,
        bool IsCashOnDelivery,
        DateTimeOffset? OrderDate,
        List<OrderItemDto> Items,
        Guid? CourierId,
        ShippingAddressDto? ShippingAddress,
        MoneyDto? ShippingFee,
        MoneyDto? TaxAmount,
        OrderDiscountDto? Discount,
        InitialPaymentDto? Payment,
        string? Notes,
        string? DeliveryInstructions);

    public record UpdateOrderFinancialsCommand(
        List<OrderItemDto> Items,
        MoneyDto? ShippingFee,
        MoneyDto? TaxAmount,
        OrderDiscountDto? Discount);

    public record OrderItemDto(
        Guid? Id,
        Guid ProductVariantId,
        int Quantity,
        OrderDiscountDto? Discount);

    public record OrderDiscountDto(
        decimal Value,
        DiscountType Type,
        string? Reason);

    public record OrderDetailsDto(
        Guid Id,
        string OrderNumber,
        CustomerDto Customer,
        Guid? CourierId,
        string? TrackingNumber,
        string ShippingAddressStreet,
        string ShippingAddressCity,
        string ShippingAddressDistrict,
        string ShippingAddressState,
        string ShippingAddressPostalCode,
        string ShippingAddressCountry,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        bool IsCashOnDelivery,
        string? DeliveryInstructions,
        string? Notes,
        decimal SubTotal,
        decimal DiscountAmount,
        string? DiscountReason,
        decimal ShippingCost,
        decimal TaxAmount,
        decimal GrandTotal,
        decimal TotalPaid,
        decimal BalanceAmount,
        string BaseCurrency,
        DateTimeOffset? OrderDate,
        DateTimeOffset CreatedOnUtc,
        List<OrderItemDetailsDto> Items,
        List<OrderPaymentDetailsDto> Payments
    );

    //public record CustomerDetailsDto(
    //    Guid Id,
    //    string Name,
    //    string PrimaryPhone,
    //    string? SecondaryPhone,
    //    string? Email
    //);

    public record OrderItemDetailsDto(
        Guid Id,
        Guid ProductVariantId,
        string ProductName,
        string Sku,
        string? ImageUrl,
        int AvailableQuantity,
        int Quantity,
        int AllocatedQuantity,
        int PendingQuantity,
        int IncomingStock,
        decimal UnitPrice,
        decimal DiscountAmount,
        string? DiscountReason,
        decimal LineTotal,
        OrderItemStatus Status
    );

    public record OrderPaymentDetailsDto(
        Guid Id,
        decimal Amount,
        PaymentMethod Method,
        string? Reference,
        DateTimeOffset PaymentDate,
        string? GatewayName,
        string? GatewayTransactionId,
        string? GatewayPaymentStatus
    );

    public record UpdateOrderLogisticsCommand(
        Guid? CourierId,
        ShippingAddressDto? ShippingAddress,
        string? DeliveryInstructions);

    public record ShippingAddressDto(
        string? Street,
        string? City,
        string? District,
        string? State,
        string? PostalCode,
        string? Country);

    public record InitialPaymentDto(
        MoneyDto Amount,
        PaymentMethod Method,
        string? Reference,
        DateTimeOffset PaymentDate);

    public record AddPaymentCommand(
        decimal Amount,
        string Currency,
        PaymentMethod Method,
        string? Reference,
        DateTimeOffset PaymentDate);

    public record MoneyDto(decimal Amount, string Currency = "LKR");

    public class AddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{City}, {District}";
        }
    }

    public record UpdateOrderNotesCommand(string? Notes);

    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PrimaryPhone { get; set; } = string.Empty;
        public string? SecondaryPhone { get; set; }
        public AddressDto? Address { get; set; }
        public string? DeliveryInstructions { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class CreateCustomerCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PrimaryPhone { get; set; } = string.Empty;
        public string? SecondaryPhone { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? DeliveryInstructions { get; set; }
        public string? Notes { get; set; }
    }

    public class CustomerOrderHistoryResponse
    {
        public int TotalOrdersCount { get; set; }
        public List<CustomerOrderSummaryDto> RecentOrders { get; set; } = new();
    }

    public class CustomerOrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTimeOffset? OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public string GrandTotalCurrency { get; set; } = string.Empty;
        public decimal BalanceAmount { get; set; }
    }

    public record VariantAttributeDto(
        string Name,
        string Value
    );

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
        public int IncomingStock {  get; set; }
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

    public record CourierDto(
        Guid Id,
        string Name,
        string? ContactPerson,
        string? PrimaryPhone,
        string? SecondaryPhone,
        string? WebsiteUrl,
        bool IsActive);

    public record AllocateOrderItemQuantityCommand(
        int QuantityToAllocate);

    public record CreateOrderProcurementTaskCommand(
        Guid OrderItemId,
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority);

    public record CreateOrderProductionTaskCommand(
        Guid OrderItemId,
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority);

    public record CancelOrderCommand(string? Reason);

    public record FailDeliveryCommand(bool IsReturning, string? Reason);

    public record ShipOrderCommand(Guid CourierId, string? TrackingNumber);

    public record ReceiveReturnRequest(bool IsReceived, string? Reason);

    public record ProcessReturnRequest(List<ReturnItemQuantity> ItemsToReturn, string? Reason);

    public record ReturnItemQuantity(Guid OrderItemId, int Quantity);
}
