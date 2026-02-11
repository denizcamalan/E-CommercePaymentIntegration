using System.ComponentModel.DataAnnotations.Schema;
using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.Entities;

public class OrderedProducts : BaseEntity<Guid>
{
    public string ProductId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Currency Currency { get; set; }

    public Category Category { get; set; }

    public int Quantity { get; set; }
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    virtual public Orders Orders { get; set; }
}
