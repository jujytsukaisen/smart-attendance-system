using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core fluent configuration for the <see cref="AttendanceEvent"/> entity.
/// Maps every daily scan event to the <c>AttendanceEvents</c> table.
/// </summary>
public sealed class AttendanceEventConfiguration : IEntityTypeConfiguration<AttendanceEvent>
{
    public void Configure(EntityTypeBuilder<AttendanceEvent> builder)
    {
        builder.ToTable("AttendanceEvents");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        // Relationship IDs
        builder.Property(e => e.AttendanceId).IsRequired();
        builder.Property(e => e.EmployeeId).IsRequired();
        builder.Property(e => e.CompanyId).IsRequired();

        // Core fields
        builder.Property(e => e.Timestamp).IsRequired();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        // Auditing
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.DeletedBy).HasMaxLength(256);

        // Indexes — optimised for the most frequent queries
        builder.HasIndex(e => new { e.AttendanceId, e.Timestamp })
            .HasDatabaseName("IX_AttendanceEvents_AttendanceId_Timestamp");

        builder.HasIndex(e => new { e.EmployeeId, e.Timestamp })
            .HasDatabaseName("IX_AttendanceEvents_EmployeeId_Timestamp");

        builder.HasIndex(e => new { e.CompanyId, e.Timestamp })
            .HasDatabaseName("IX_AttendanceEvents_CompanyId_Timestamp");

        // Relationships
        builder.HasOne(e => e.Attendance)
            .WithMany(a => a.Events)
            .HasForeignKey(e => e.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
