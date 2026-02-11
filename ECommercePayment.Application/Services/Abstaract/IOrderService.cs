using ECommercePayment.Domain.DTOs.Request;
using ECommercePayment.Domain.DTOs.Response;

namespace ECommercePayment.Application.Services.Abstaract;

public interface IOrderService
{
    Task<BaseResponse<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    Task<BaseResponse<CompleteOrderResponse>> CompleteOrderAsync(string preOrderId, CompleteOrderRequest request, CancellationToken cancellationToken = default);
}
