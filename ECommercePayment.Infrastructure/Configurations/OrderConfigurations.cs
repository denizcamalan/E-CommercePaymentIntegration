using ECommercePayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePayment.Infrastructure.Configurations;

public class OrderConfigurations : IEntityTypeConfiguration<Orders>
{
    public void Configure(EntityTypeBuilder<Orders> builder)
    {
        builder.HasMany(o => o.OrderedProducts)
                .WithOne(op => op.Orders)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(nameof(Orders), "Order");
    }

}
