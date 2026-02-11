using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.DTOs;

public class ProductModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Currency Currency { get; set; }

    public Category Category { get; set; }

    public int Quantity { get; set; }
}
