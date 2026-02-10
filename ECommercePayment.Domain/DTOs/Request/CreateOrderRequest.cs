namespace ECommercePayment.Domain.DTOs.Request;

public class CreateOrderRequest
{
    public string UserId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal? TotalPrice{ get; set; }

    public string Currency { get; set; } = string.Empty;
}
