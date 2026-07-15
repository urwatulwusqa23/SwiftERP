using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays", "hr");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Date).IsRequired();
        builder.Property(h => h.Name).HasMaxLength(256).IsRequired();

        builder.Ignore(h => h.DomainEvents);
    }
}
