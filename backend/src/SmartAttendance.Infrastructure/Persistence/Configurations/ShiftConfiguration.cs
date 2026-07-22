using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");

        // Primary Key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.ShiftType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.IsNightShift)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.IsFlexible)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.MinimumWorkingHours)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(s => s.MaximumWorkingHours)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(s => s.GracePeriodMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.AllowEarlyCheckInMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.AllowLateCheckOutMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.WorkScheduleId)
            .IsRequired();

        // Auditing
        builder.Property(s => s.CompanyId).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(s => s.CreatedBy).HasMaxLength(256);
        builder.Property(s => s.UpdatedBy).HasMaxLength(256);
        builder.Property(s => s.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(s => new { s.CompanyId, s.Code })
            .IsUnique()
            .HasDatabaseName("IX_Shifts_CompanyId_Code");

        builder.HasIndex(s => s.WorkScheduleId)
            .HasDatabaseName("IX_Shifts_WorkScheduleId");

        builder.HasIndex(s => s.CompanyId)
            .HasDatabaseName("IX_Shifts_CompanyId");

        // Relationships
        builder.HasOne(s => s.WorkSchedule)
            .WithMany(ws => ws.Shifts)
            .HasForeignKey(s => s.WorkScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Breaks)
            .WithOne(b => b.Shift)
            .HasForeignKey(b => b.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // Private field access for breaks collection
        builder.Navigation(s => s.Breaks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

