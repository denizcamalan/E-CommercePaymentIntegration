namespace ECommercePayment.Domain.DTOs.Request;

public class CompleteOrderRequest
{
    /// <summary>
    /// İşlem yapılan sepetin kimliği (uygulama tarafı)
    /// </summary>
    public string CartId { get; set; } = string.Empty;

    /// <summary>
    /// Balance Management tarafındaki PreOrder/Order kimliği
    /// </summary>
    public string PreOrderId { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının ödeme onayını temsil eder.
    /// </summary>
    public bool PaymentConfirmation { get; set; }
}
