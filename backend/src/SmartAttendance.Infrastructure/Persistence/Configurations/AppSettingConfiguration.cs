using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");

        // Primary Key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        // Tenant
        builder.Property(s => s.CompanyId).IsRequired();

        // Properties
        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(4000); // Allow longer values (e.g. JSON sub-sections)

        // BaseEntity properties
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique constraint: one key per company per category
        builder.HasIndex(s => new { s.CompanyId, s.Category, s.Key })
            .IsUnique()
            .HasDatabaseName("IX_AppSettings_Company_Category_Key");

        builder.HasIndex(s => new { s.CompanyId, s.Category })
            .HasDatabaseName("IX_AppSettings_Company_Category");
    }
}
