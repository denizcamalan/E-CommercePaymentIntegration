namespace ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;

public class CancelResponse
{
    public OrderModel Order { get; set; } = default!;

    public UpdatedBalanceModel UpdatedBalance { get; set; } = default!;
}
