namespace ECommercePayment.Domain.Entities;

public class Products : BaseEntity<Guid>
{
    public string ExternalProductId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Stock { get; set; }
}
