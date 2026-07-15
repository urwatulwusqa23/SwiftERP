using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "identity");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(128).IsRequired();
        builder.Property(r => r.IsSystemRole).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();

        builder.Ignore(r => r.DomainEvents);

        builder.OwnsMany(r => r.Permissions, permissions =>
        {
            permissions.ToTable("RoleModulePermissions", "identity");
            permissions.WithOwner().HasForeignKey("RoleId");
            permissions.Property<Guid>("Id").ValueGeneratedOnAdd();
            permissions.HasKey("Id");

            permissions.Property(p => p.Module).HasConversion<string>().HasMaxLength(32).IsRequired();
            permissions.Property(p => p.AccessLevel).HasConversion<string>().HasMaxLength(16).IsRequired();
        });

        builder.Navigation(r => r.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
