using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        // Primary Key
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.IsSystemRole)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        // Auditing
        builder.Property(r => r.CompanyId).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt);
        builder.Property(r => r.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.CreatedBy).HasMaxLength(256);
        builder.Property(r => r.UpdatedBy).HasMaxLength(256);
        builder.Property(r => r.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(r => new { r.CompanyId, r.Code })
            .IsUnique()
            .HasDatabaseName("IX_Roles_CompanyId_Code");

        builder.HasIndex(r => r.CompanyId)
            .HasDatabaseName("IX_Roles_CompanyId");

        builder.HasIndex(r => r.IsSystemRole)
            .HasDatabaseName("IX_Roles_IsSystemRole")
            .HasFilter("[IsSystemRole] = 1");

        // Many-to-many: Role ↔ Permission (join table)
        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RolePermission",
                j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("RoleId", "PermissionId");
                    j.ToTable("RolePermissions");
                    j.HasIndex("PermissionId").HasDatabaseName("IX_RolePermissions_PermissionId");
                });

        // Private field access for permissions collection
        builder.Navigation(r => r.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

