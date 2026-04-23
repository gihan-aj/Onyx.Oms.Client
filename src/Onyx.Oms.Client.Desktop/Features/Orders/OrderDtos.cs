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
        public DateTime PaymentDate { get; set; }
    }

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
        string? Notes);

    public record OrderItemDto(
        Guid ProductVariantId,
        int Quantity,
        OrderDiscountDto? Discount);

    public record OrderDiscountDto(
        decimal Value,
        DiscountType Type,
        string? Reason);

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
        DateTime PaymentDate);

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

    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PrimaryPhone { get; set; } = string.Empty;
        public string? SecondaryPhone { get; set; }
        public AddressDto? Address { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
