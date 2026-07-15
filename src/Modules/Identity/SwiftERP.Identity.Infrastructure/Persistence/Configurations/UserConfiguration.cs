using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftERP.Identity.Domain.Users;

namespace SwiftERP.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "identity");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.EmployeeId).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(1024).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.EmployeeId).IsUnique();

        builder.Ignore(u => u.DomainEvents);

        builder.OwnsMany(u => u.Roles, roles =>
        {
            roles.ToTable("UserRoles", "identity");
            roles.WithOwner().HasForeignKey("UserId");
            roles.Property<Guid>("Id").ValueGeneratedOnAdd();
            roles.HasKey("Id");

            roles.Property(r => r.RoleId).IsRequired();
        });

        builder.Navigation(u => u.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
