using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public class OrderPaymentLineItem
    {
        public Guid Id { get; private set; }
        public string Amount { get; private set; }
        public string Currency { get; private set; } = "LKR";
        public PaymentMethod Method { get; private set; }
        public string? Reference { get; private set; }
        public DateTime PaymentDate { get; private set; }
        public string? GatewayName { get; private set; }
        public string? GatewayTransactionId { get; private set; }
        public string? GatewayPaymentStatus { get; private set; }

        public OrderPaymentLineItem(OrderPaymentDetailsDto orderPaymentDetails, string currency)
        {
            Id = orderPaymentDetails.Id;
            Amount = orderPaymentDetails.Amount.ToString("N2");
            Currency = currency;
            Method = orderPaymentDetails.Method;
            Reference = orderPaymentDetails.Reference;
            PaymentDate = orderPaymentDetails.PaymentDate.ToLocalTime().DateTime;
            GatewayName = orderPaymentDetails.GatewayName;
            GatewayTransactionId = orderPaymentDetails.GatewayTransactionId;
            GatewayPaymentStatus = orderPaymentDetails.GatewayPaymentStatus;
        }
    }
}
