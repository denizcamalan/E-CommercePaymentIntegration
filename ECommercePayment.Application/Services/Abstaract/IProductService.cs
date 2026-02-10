using ECommercePayment.Domain.DTOs.Response;

namespace ECommercePayment.Application.Services.Abstaract;

public interface IProductService
{
    Task<BaseResponse<List<ProductResponse>>> GetProductsAsync(CancellationToken cancellationToken = default);
}
