using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.DTOs.Response;

public class CreateOrderResponse
{
    public string PreOrderId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public List<ProductModel> Products { get; set; } = new List<ProductModel>();

}

