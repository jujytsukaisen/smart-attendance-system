using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");

        // Primary Key
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(1000);

        builder.Property(h => h.HolidayType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(h => h.StartDate)
            .IsRequired();

        builder.Property(h => h.EndDate)
            .IsRequired();

        builder.Property(h => h.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(h => h.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(h => h.BranchId);

        // Auditing
        builder.Property(h => h.CompanyId).IsRequired();
        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.UpdatedAt);
        builder.Property(h => h.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(h => h.CreatedBy).HasMaxLength(256);
        builder.Property(h => h.UpdatedBy).HasMaxLength(256);
        builder.Property(h => h.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(h => h.CompanyId)
            .HasDatabaseName("IX_Holidays_CompanyId");

        builder.HasIndex(h => new { h.CompanyId, h.StartDate, h.EndDate })
            .HasDatabaseName("IX_Holidays_CompanyId_DateRange");

        builder.HasIndex(h => h.BranchId)
            .HasDatabaseName("IX_Holidays_BranchId")
            .HasFilter("[BranchId] IS NOT NULL");

        // Relationships
        builder.HasOne(h => h.Branch)
            .WithMany()
            .HasForeignKey(h => h.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

