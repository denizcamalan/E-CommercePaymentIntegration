namespace ECommercePayment.Integrations.Models.Request;

public class PreOrderRequest
{
        public int Amount { get; set; }
        public string OrderId { get; set; } = string.Empty;

}
