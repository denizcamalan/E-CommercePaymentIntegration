using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Enums;
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
            Currency = Enum.TryParse<Currency>(source.Currency, true, out var currency) ? currency : Currency.TRY,
            Category = Enum.TryParse<Category>(source.Category, true, out var category) ? category : Category.ELECTRONICS,
            Stock = source.Stock
        };
}

