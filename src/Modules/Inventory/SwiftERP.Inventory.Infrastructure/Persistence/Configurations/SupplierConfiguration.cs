using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Inventory.Domain.Suppliers;

namespace SwiftERP.Inventory.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(256).IsRequired();
        builder.Property(s => s.ContactEmail).HasMaxLength(256).IsRequired();

        builder.Ignore(s => s.DomainEvents);
    }
}
