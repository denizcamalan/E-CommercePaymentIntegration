using System.Reflection;
using ECommercePayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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

public class EcommercePaymentDbContextFactory 
    : IDesignTimeDbContextFactory<EcommercePaymentDbContext>
{
    public EcommercePaymentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EcommercePaymentDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=EcommercePayment;Username=postgres;Password=Ecommerce2026");

        return new EcommercePaymentDbContext(optionsBuilder.Options);
    }
}
