namespace ECommercePayment.Domain.DTOs.Request;

public class CreateOrderRequest
{
    public string UserId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    /// <summary>
    /// İsteğe bağlı olarak istemciden doğrudan toplam tutar gönderilebilir.
    /// Eğer doldurulmazsa, ileride ürün fiyatı üzerinden hesaplanacak şekilde genişletilebilir.
    /// </summary>
    public decimal? TotalAmount { get; set; }

    public string Currency { get; set; } = string.Empty;
}
