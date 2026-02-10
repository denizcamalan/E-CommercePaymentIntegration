namespace ECommercePayment.Domain.DTOs.Response;

public class CreateOrderResponse
{
    public string CartId { get; set; } = string.Empty;

    public string PreOrderId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal BlockedAmount { get; set; }

    public DateTime ExpiresAt { get; set; }
}

