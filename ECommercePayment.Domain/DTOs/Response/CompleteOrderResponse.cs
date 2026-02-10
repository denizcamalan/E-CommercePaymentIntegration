namespace ECommercePayment.Domain.DTOs.Response;

public class CompleteOrderResponse
{
    public string OrderId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CompletedAt { get; set; }
}

