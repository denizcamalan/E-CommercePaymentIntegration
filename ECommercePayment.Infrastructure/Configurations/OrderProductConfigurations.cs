using ECommercePayment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePayment.Infrastructure.Configurations;

public class OrderProductConfigurations : IEntityTypeConfiguration<OrderedProducts>
{
    public void Configure(EntityTypeBuilder<OrderedProducts> builder)
    {
        builder.ToTable(nameof(OrderedProducts), "Order");
    }
}
