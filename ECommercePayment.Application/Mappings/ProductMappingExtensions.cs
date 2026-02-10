using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;

namespace ECommercePayment.Application.Mappings;

public static class ProductMappingExtensions
{
    public static ProductResponse ToProductResponse(this ProductsResponse source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Price = source.Price,
            Currency = source.Currency,
            Category = source.Category,
            Stock = source.Stock
        };
}

