using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Sales.Domain.SaleOrders;

namespace SwiftERP.Sales.Infrastructure.Persistence.Configurations;

public class SaleOrderConfiguration : IEntityTypeConfiguration<SaleOrder>
{
    public void Configure(EntityTypeBuilder<SaleOrder> builder)
    {
        builder.ToTable("SaleOrders");
        builder.HasKey(so => so.Id);

        builder.Property(so => so.CustomerId).IsRequired();
        builder.Property(so => so.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(so => so.PaymentStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(so => so.CreatedAtUtc).IsRequired();

        builder.Ignore(so => so.DomainEvents);
        builder.Ignore(so => so.Total);

        builder.OwnsMany(so => so.Lines, lines =>
        {
            lines.ToTable("SaleOrderLines");
            lines.WithOwner().HasForeignKey("SaleOrderId");
            lines.Property<Guid>("Id").ValueGeneratedOnAdd();
            lines.HasKey("Id");

            lines.Property(l => l.ProductId).IsRequired();
            lines.Property(l => l.Quantity).IsRequired();
            lines.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Navigation(so => so.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
