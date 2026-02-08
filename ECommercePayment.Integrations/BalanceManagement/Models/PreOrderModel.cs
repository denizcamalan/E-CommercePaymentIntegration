namespace ECommercePayment.Integrations.BalanceManagement.Models;

public class OrderModel
{
    public string OrderId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}