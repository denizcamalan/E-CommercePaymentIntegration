using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;

namespace ECommercePayment.Integrations.Services;

public interface IBalanceManagementService
{
    UserBalanceResponse GetUserBalance(string userId);
    PreOrderResponse PreOrder(PreOrderRequest request);
    CompleteResponse CompleteOrder(CompleteRequest request);
    CancelResponse CancelOrder(CancelRequest request);
    ProductsResponse GetProducts();
}
