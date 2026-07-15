using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Attendance;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords", "hr");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmployeeId).IsRequired();
        builder.Property(a => a.Date).IsRequired();
        builder.Property(a => a.ClockInUtc).IsRequired();
        builder.Property(a => a.ClockOutUtc);

        builder.HasIndex(a => a.EmployeeId);

        builder.Ignore(a => a.DomainEvents);
        builder.Ignore(a => a.WorkedHours);
        builder.Ignore(a => a.OvertimeHours);
    }
}
