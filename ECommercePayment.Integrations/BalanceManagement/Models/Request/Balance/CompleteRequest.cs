using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;

public class CompleteRequest
{
    [Required]
    public string orderId { get; set; } = string.Empty;
}
