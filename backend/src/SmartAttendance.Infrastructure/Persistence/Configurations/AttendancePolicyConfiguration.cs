using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class AttendancePolicyConfiguration : IEntityTypeConfiguration<AttendancePolicy>
{
    public void Configure(EntityTypeBuilder<AttendancePolicy> builder)
    {
        builder.ToTable("AttendancePolicies");

        // Primary Key
        builder.HasKey(ap => ap.Id);
        builder.Property(ap => ap.Id)
            .ValueGeneratedNever();

        // Identity
        builder.Property(ap => ap.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ap => ap.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ap => ap.Description)
            .HasMaxLength(1000);

        // Attendance Rules
        builder.Property(ap => ap.AttendanceMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ap => ap.RequireFaceRecognition).IsRequired();
        builder.Property(ap => ap.RequireGPS).IsRequired();
        builder.Property(ap => ap.RequireApprovedDevice).IsRequired();
        builder.Property(ap => ap.RequireCompanyNetwork).IsRequired();
        builder.Property(ap => ap.AllowMobileCheckIn).IsRequired();
        builder.Property(ap => ap.AllowDesktopCheckIn).IsRequired();
        builder.Property(ap => ap.AllowWebCheckIn).IsRequired();
        builder.Property(ap => ap.AllowOfflineAttendance).IsRequired();

        builder.Property(ap => ap.AllowedRadiusMeters)
            .HasPrecision(10, 2);

        builder.Property(ap => ap.AllowMultipleCheckIn).IsRequired();
        builder.Property(ap => ap.AllowManualAttendance).IsRequired();
        builder.Property(ap => ap.RequireManagerApproval).IsRequired();

        builder.Property(ap => ap.MaximumDistanceMeters)
            .HasPrecision(10, 2);

        // New Rules
        builder.Property(ap => ap.LateToleranceMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(ap => ap.EarlyLeaveToleranceMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(ap => ap.GracePeriodMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(ap => ap.OvertimeThresholdMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(ap => ap.MinimumOvertimeMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(ap => ap.MaximumOvertimeMinutes).IsRequired().HasDefaultValue(0);

        builder.Property(ap => ap.AutoCheckoutEnabled).IsRequired().HasDefaultValue(false);
        builder.Property(ap => ap.AutoCheckoutTime).IsRequired(false);

        builder.Property(ap => ap.MultipleShiftsSupport).IsRequired().HasDefaultValue(false);

        builder.Property(ap => ap.RoundTimeRule)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SmartAttendance.Domain.Enums.RoundTimeRule.None);

        builder.Property(ap => ap.MissingPunchPolicy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SmartAttendance.Domain.Enums.MissingPunchPolicy.Reject);

        builder.Property(ap => ap.WeekendPolicy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SmartAttendance.Domain.Enums.WeekendPolicy.Regular);

        builder.Property(ap => ap.HolidayPolicy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SmartAttendance.Domain.Enums.HolidayPolicy.Regular);

        builder.Property(ap => ap.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ap => ap.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        // Auditing
        builder.Property(ap => ap.CompanyId).IsRequired();
        builder.Property(ap => ap.CreatedAt).IsRequired();
        builder.Property(ap => ap.UpdatedAt);
        builder.Property(ap => ap.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(ap => ap.CreatedBy).HasMaxLength(256);
        builder.Property(ap => ap.UpdatedBy).HasMaxLength(256);
        builder.Property(ap => ap.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(ap => new { ap.CompanyId, ap.Code })
            .IsUnique()
            .HasDatabaseName("IX_AttendancePolicies_CompanyId_Code");

        builder.HasIndex(ap => ap.CompanyId)
            .HasDatabaseName("IX_AttendancePolicies_CompanyId");

        // Filtered index: only one default policy per company
        builder.HasIndex(ap => new { ap.CompanyId, ap.IsDefault })
            .HasDatabaseName("IX_AttendancePolicies_CompanyId_IsDefault")
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0");

        // Relationships — Employees collection uses backing field
        builder.HasMany(ap => ap.Employees)
            .WithOne(e => e.AttendancePolicy)
            .HasForeignKey(e => e.AttendancePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(ap => ap.Employees).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

