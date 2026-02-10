namespace ECommercePayment.Infrastructure.UOW;

public interface IUOW
{
        EcommercePaymentDbContext DbContext { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
