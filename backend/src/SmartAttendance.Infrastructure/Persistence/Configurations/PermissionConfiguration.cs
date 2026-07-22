using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        // Primary Key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.IsSystemPermission)
            .IsRequired()
            .HasDefaultValue(false);

        // BaseEntity properties
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);

        // Indexes
        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Code");

        builder.HasIndex(p => p.Module)
            .HasDatabaseName("IX_Permissions_Module");

        builder.HasIndex(p => p.IsSystemPermission)
            .HasDatabaseName("IX_Permissions_IsSystemPermission")
            .HasFilter("[IsSystemPermission] = 1");

        // NOTE: Permission is a global entity (derives from BaseEntity, not BaseTenantEntity).
        // It has NO CompanyId and NO tenant filter.
        // The soft-delete filter is applied via the global filter extension.
    }
}

