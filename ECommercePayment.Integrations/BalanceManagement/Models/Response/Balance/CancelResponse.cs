namespace ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;

public class CancelResponse
{
    public OrderModel Order { get; set; }

    public BalanceModel UpdatedBalance { get; set; }
}
