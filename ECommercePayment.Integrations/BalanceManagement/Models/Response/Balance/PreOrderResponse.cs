using ECommercePayment.Integrations.BalanceManagement.Models;

namespace ECommercePayment.Integrations.Models.Response.Balance;

public class PreOrderResponse
{
    public OrderModel PreOrder { get; set; }
    public UpdatedBalanceModel UpdatedBalance { get; set; }
}


