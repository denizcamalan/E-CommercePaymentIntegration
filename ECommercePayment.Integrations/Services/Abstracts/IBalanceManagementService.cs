using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;

namespace ECommercePayment.Integrations.Services;

public interface IBalanceManagementService
{
    Task<BaseResponse<UserBalanceResponse>> GetUserBalance();
    Task<BaseResponse<PreOrderResponse>> PreOrder(PreOrderRequest request);
    Task<BaseResponse<CompleteResponse>> CompleteOrder(CompleteRequest request);
    Task<BaseResponse<CancelResponse>> CancelOrder(CancelRequest request);
    Task<BaseResponse<List<ProductsResponse>>> GetProducts();
}
