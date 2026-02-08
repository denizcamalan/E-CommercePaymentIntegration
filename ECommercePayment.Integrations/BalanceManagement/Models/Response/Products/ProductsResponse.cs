namespace ECommercePayment.Integrations.Models.Response.Products;

public class ProductsResponse
{
     public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Stock { get; set; }
}
