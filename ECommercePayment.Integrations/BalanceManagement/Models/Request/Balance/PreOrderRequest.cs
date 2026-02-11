using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;

public class PreOrderRequest
{
        [Required]
        public int amount { get; set; }
        [Required]
        public string orderId { get; set; } = string.Empty;
}
