using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Inventory.Domain.PurchaseOrders;

namespace SwiftERP.Inventory.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(po => po.Id);

        builder.Property(po => po.SupplierId).IsRequired();
        builder.Property(po => po.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(po => po.CreatedAtUtc).IsRequired();

        builder.Ignore(po => po.DomainEvents);

        builder.OwnsMany(po => po.Lines, lines =>
        {
            lines.ToTable("PurchaseOrderLines");
            lines.WithOwner().HasForeignKey("PurchaseOrderId");
            lines.Property<Guid>("Id").ValueGeneratedOnAdd();
            lines.HasKey("Id");

            lines.Property(l => l.ProductId).IsRequired();
            lines.Property(l => l.Quantity).IsRequired();
            lines.Property(l => l.UnitCost).HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Navigation(po => po.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
