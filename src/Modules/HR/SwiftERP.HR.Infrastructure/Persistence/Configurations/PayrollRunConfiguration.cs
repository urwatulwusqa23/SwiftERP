using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Payroll;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns", "hr");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Year).IsRequired();
        builder.Property(p => p.Month).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.Total);

        builder.OwnsMany(p => p.Lines, lines =>
        {
            lines.ToTable("PayrollRunLines", "hr");
            lines.WithOwner().HasForeignKey("PayrollRunId");
            lines.Property<Guid>("Id").ValueGeneratedOnAdd();
            lines.HasKey("Id");

            lines.Property(l => l.EmployeeId).IsRequired();
            lines.Property(l => l.Amount).HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Navigation(p => p.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
