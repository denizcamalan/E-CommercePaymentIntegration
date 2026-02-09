using System.ComponentModel.DataAnnotations;
namespace ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;

public class PreOrderResponse
{
    public OrderModel PreOrder { get; set; }
    public BalanceModel UpdatedBalance { get; set; }
}