using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances", "hr");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.EmployeeId).IsRequired();
        builder.Property(b => b.LeaveType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(b => b.Year).IsRequired();
        builder.Property(b => b.TotalDays).IsRequired();
        builder.Property(b => b.UsedDays).IsRequired();

        builder.HasIndex(b => new { b.EmployeeId, b.LeaveType, b.Year }).IsUnique();

        builder.Ignore(b => b.DomainEvents);
        builder.Ignore(b => b.AvailableDays);
    }
}
