using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.DTOs.Request;

public class CreateOrderRequest
{
    public string UserId { get; set; } = string.Empty;

    public List<ProductOrderItem> SelectedProducts { get; set; } = new List<ProductOrderItem>();
}

public class ProductOrderItem
{
    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal? TotalPrice{ get; set; }

    public Currency Currency { get; set; }
}
