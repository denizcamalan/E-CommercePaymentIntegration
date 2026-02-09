using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.Entities
{
    public class Orders : BaseEntity<Guid>
    {
        public required string OrderId { get; set; }
        public required decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
