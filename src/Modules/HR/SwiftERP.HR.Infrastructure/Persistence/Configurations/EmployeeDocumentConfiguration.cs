using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Infrastructure.Persistence.Configurations;

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments", "hr");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.EmployeeId).IsRequired();
        builder.Property(d => d.DocumentType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(d => d.FileName).HasMaxLength(256).IsRequired();
        builder.Property(d => d.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(d => d.SizeBytes).IsRequired();
        builder.Property(d => d.UploadedAtUtc).IsRequired();

        builder.HasIndex(d => d.EmployeeId);

        builder.Ignore(d => d.DomainEvents);
    }
}
