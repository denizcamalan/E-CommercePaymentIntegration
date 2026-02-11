namespace ECommercePayment.Infrastructure.UOW;

public class UOW : IUOW
{
    private readonly EcommercePaymentDbContext _dbContext;

    public UOW(EcommercePaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public EcommercePaymentDbContext DbContext => _dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
