using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;

public class PreOrderRequest
{
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string OrderId { get; set; } = string.Empty;
}
