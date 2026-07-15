using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests", "hr");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.EmployeeId).IsRequired();
        builder.Property(r => r.LeaveType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.StartDate).IsRequired();
        builder.Property(r => r.EndDate).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(1024);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.RequestedAtUtc).IsRequired();
        builder.Property(r => r.DecidedAtUtc);
        builder.Property(r => r.DecisionNote).HasMaxLength(1024);

        builder.HasIndex(r => r.EmployeeId);

        builder.Ignore(r => r.DomainEvents);
        builder.Ignore(r => r.TotalDays);
    }
}
