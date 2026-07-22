using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");

        // Primary Key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        // Relationship IDs
        builder.Property(a => a.EmployeeId).IsRequired();
        builder.Property(a => a.AttendanceDeviceId).IsRequired();
        builder.Property(a => a.AttendancePolicyId).IsRequired();
        builder.Property(a => a.ShiftId).IsRequired();

        // Check In
        builder.Property(a => a.CheckInTime).IsRequired();

        builder.Property(a => a.CheckInMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.CheckInLatitude).HasPrecision(9, 6);
        builder.Property(a => a.CheckInLongitude).HasPrecision(9, 6);
        builder.Property(a => a.CheckInIPAddress).HasMaxLength(50);
        builder.Property(a => a.CheckInDeviceInfo).HasMaxLength(500);
        builder.Property(a => a.CheckInPhotoReference).HasMaxLength(1000);

        // Check Out
        builder.Property(a => a.CheckOutTime);

        builder.Property(a => a.CheckOutMethod)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.CheckOutLatitude).HasPrecision(9, 6);
        builder.Property(a => a.CheckOutLongitude).HasPrecision(9, 6);
        builder.Property(a => a.CheckOutIPAddress).HasMaxLength(50);
        builder.Property(a => a.CheckOutDeviceInfo).HasMaxLength(500);
        builder.Property(a => a.CheckOutPhotoReference).HasMaxLength(1000);

        // Calculated Information
        builder.Property(a => a.AttendanceStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.WorkingMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(a => a.LateMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(a => a.EarlyLeaveMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(a => a.OvertimeMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(a => a.BreakMinutes).IsRequired().HasDefaultValue(0);

        // Verification Statuses
        builder.Property(a => a.FaceVerificationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.GPSVerificationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.DeviceVerificationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.ManagerApprovalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Auditing
        builder.Property(a => a.CompanyId).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(a => a.CreatedBy).HasMaxLength(256);
        builder.Property(a => a.UpdatedBy).HasMaxLength(256);
        builder.Property(a => a.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes — optimized for common attendance queries
        builder.HasIndex(a => new { a.EmployeeId, a.CheckInTime })
            .HasDatabaseName("IX_Attendances_EmployeeId_CheckInTime");

        builder.HasIndex(a => new { a.CompanyId, a.CheckInTime })
            .HasDatabaseName("IX_Attendances_CompanyId_CheckInTime");

        builder.HasIndex(a => new { a.CompanyId, a.AttendanceStatus })
            .HasDatabaseName("IX_Attendances_CompanyId_Status")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(a => a.ShiftId)
            .HasDatabaseName("IX_Attendances_ShiftId");

        builder.HasIndex(a => a.AttendanceDeviceId)
            .HasDatabaseName("IX_Attendances_AttendanceDeviceId");

        // Relationships
        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AttendanceDevice)
            .WithMany()
            .HasForeignKey(a => a.AttendanceDeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AttendancePolicy)
            .WithMany()
            .HasForeignKey(a => a.AttendancePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Shift)
            .WithMany()
            .HasForeignKey(a => a.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

