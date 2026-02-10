namespace ECommercePayment.Domain.DTOs.Request;

public class CompleteOrderRequest
{
    public string CartId { get; set; } = string.Empty;
    public bool PaymentConfirmation { get; set; }
}
