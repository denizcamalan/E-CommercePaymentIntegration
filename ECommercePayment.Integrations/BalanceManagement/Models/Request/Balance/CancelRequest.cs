using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;

public class CancelRequest
{
    [Required]
    public string OrderId { get; set; } = string.Empty;
}
