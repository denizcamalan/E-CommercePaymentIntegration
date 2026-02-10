using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.Entities
{
    public class Orders : BaseEntity<Guid>
    {
        public required string UserId { get; set; }
        public required decimal TotallPrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; private set; }
        public ICollection<OrderedProducts> OrderedProducts { get; set; } = new List<OrderedProducts>();
    }
}
