using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.Entities
{
    public class Orders : BaseEntity<Guid>
    {
        public string UserId { get; set; }
        public decimal TotallPrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public ICollection<OrderedProducts> OrderedProducts { get; set; } = new List<OrderedProducts>();
    }
}
