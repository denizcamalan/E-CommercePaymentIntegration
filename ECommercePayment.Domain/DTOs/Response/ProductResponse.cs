using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.DTOs.Response;

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Currency Currency { get; set; }

    public Category Category { get; set; }

    public int Stock { get; set; }
}
