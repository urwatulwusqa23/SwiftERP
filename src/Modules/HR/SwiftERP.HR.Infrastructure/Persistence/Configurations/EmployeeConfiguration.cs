using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", "hr");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.MonthlySalary).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.HireDate).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Property(e => e.PhoneNumber).HasMaxLength(32);
        builder.Property(e => e.Address).HasMaxLength(512);
        builder.Property(e => e.DateOfBirth);
        builder.Property(e => e.JobTitle).HasMaxLength(128);
        builder.Property(e => e.Department).HasMaxLength(128);
        builder.Property(e => e.ManagerId);

        builder.Ignore(e => e.DomainEvents);
    }
}
