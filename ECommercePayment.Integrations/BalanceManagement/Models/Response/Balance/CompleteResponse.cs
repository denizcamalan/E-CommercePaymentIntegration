namespace ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;

public class CompleteResponse
{
    public OrderModel Order { get; set; }
    public BalanceModel UpdatedBalance { get; set; }
}
