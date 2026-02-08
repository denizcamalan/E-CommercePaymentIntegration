namespace ECommercePayment.Integrations.BalanceManagement.Models;

public class UpdatedBalanceModel
{
    public string UserId { get; set; } = string.Empty;

    public decimal TotalBalance { get; set; }

    public decimal AvailableBalance { get; set; }

    public decimal BlockedBalance { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; }
}
