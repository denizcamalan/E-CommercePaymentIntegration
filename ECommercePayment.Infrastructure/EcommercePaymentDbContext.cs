using System.Reflection;
using ECommercePayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePayment.Infrastructure;

public class EcommercePaymentDbContext : DbContext
{
    public EcommercePaymentDbContext(DbContextOptions<EcommercePaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderedProducts> OrderedProducts { get; set; }
    public DbSet<ECommercePaymentIdentity> ECommercePaymentIdentity { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
