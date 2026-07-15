using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Finance.Domain.LedgerEntries;

namespace SwiftERP.Finance.Infrastructure.Persistence.Configurations;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        // Explicit schema so this configuration resolves to the same table regardless of which
        // DbContext's default schema is in effect (SalesDbContext also maps LedgerEntry, write-only,
        // under a different default schema — see SalesDbContext.OnModelCreating).
        builder.ToTable("LedgerEntries", "finance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Description).HasMaxLength(512).IsRequired();
        builder.Property(e => e.ReferenceId).IsRequired();
        builder.Property(e => e.OccurredAtUtc).IsRequired();

        builder.Ignore(e => e.DomainEvents);
    }
}
