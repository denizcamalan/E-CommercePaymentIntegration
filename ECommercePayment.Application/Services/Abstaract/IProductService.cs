using ECommercePayment.Domain.DTOs.Response;

namespace ECommercePayment.Application.Services.Abstaract;

public interface IProductService
{
    Task<BaseResponse<List<ProductResponse>>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// stok güncellemek için kullanılır.
    /// </summary>
    Task RefreshProductsCacheAsync(CancellationToken cancellationToken = default);
}
