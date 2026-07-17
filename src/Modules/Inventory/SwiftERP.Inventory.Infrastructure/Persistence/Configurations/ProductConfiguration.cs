using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Inventory.Domain.Products;

namespace SwiftERP.Inventory.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Explicit schema so this configuration resolves to the same table regardless of which
        // DbContext's default schema is in effect (SalesDbContext also maps Product, read-only,
        // under a different default schema — see SalesDbContext.OnModelCreating).
        builder.ToTable("Products", "inventory");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).HasMaxLength(64).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.QuantityOnHand).IsRequired();
        builder.Property(p => p.ReorderThreshold).IsRequired();
        builder.Property(p => p.SupplierId).IsRequired();

        // Maps to Postgres's xmin system column — Npgsql bumps it automatically on every UPDATE,
        // giving the same "reject a stale write" behavior SQL Server's rowversion provided.
        builder.Property(p => p.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        builder.Ignore(p => p.DomainEvents);
    }
}
