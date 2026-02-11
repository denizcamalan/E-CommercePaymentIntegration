using ECommercePayment.Integrations.BalanceManagement.Enums;

namespace ECommercePayment.Integrations.BalanceManagement.Models;

public class OrderModel
{
    public required string OrderId { get; set; }
    public required decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}