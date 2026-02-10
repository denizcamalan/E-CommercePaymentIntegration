using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.DTOs.Response;

public class CompleteOrderResponse
{
    public string OrderId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } 

    public decimal TotalAmount { get; set; }

    public DateTime CompletedAt { get; set; }
}

