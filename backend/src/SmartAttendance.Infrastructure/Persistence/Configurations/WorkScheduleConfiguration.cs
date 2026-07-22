using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class WorkScheduleConfiguration : IEntityTypeConfiguration<WorkSchedule>
{
    public void Configure(EntityTypeBuilder<WorkSchedule> builder)
    {
        builder.ToTable("WorkSchedules");

        // Primary Key
        builder.HasKey(ws => ws.Id);
        builder.Property(ws => ws.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(ws => ws.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ws => ws.Description)
            .HasMaxLength(1000);

        builder.Property(ws => ws.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ws => ws.IsFlexible)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ws => ws.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ws => ws.WeeklyWorkingHours)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(ws => ws.GracePeriodMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ws => ws.AllowEarlyCheckInMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ws => ws.AllowLateCheckOutMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        // Auditing
        builder.Property(ws => ws.CompanyId).IsRequired();
        builder.Property(ws => ws.CreatedAt).IsRequired();
        builder.Property(ws => ws.UpdatedAt);
        builder.Property(ws => ws.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(ws => ws.CreatedBy).HasMaxLength(256);
        builder.Property(ws => ws.UpdatedBy).HasMaxLength(256);
        builder.Property(ws => ws.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(ws => ws.CompanyId)
            .HasDatabaseName("IX_WorkSchedules_CompanyId");

        builder.HasIndex(ws => new { ws.CompanyId, ws.Name })
            .IsUnique()
            .HasDatabaseName("IX_WorkSchedules_CompanyId_Name");

        builder.HasIndex(ws => new { ws.CompanyId, ws.IsDefault })
            .HasDatabaseName("IX_WorkSchedules_CompanyId_IsDefault")
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0");

        // Relationships — Shifts (one-to-many)
        builder.HasMany(ws => ws.Shifts)
            .WithOne(s => s.WorkSchedule)
            .HasForeignKey(s => s.WorkScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many: WorkSchedule ↔ Employee (join table)
        builder.HasMany(ws => ws.Employees)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "WorkScheduleEmployee",
                j => j.HasOne<Employee>().WithMany().HasForeignKey("EmployeeId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<WorkSchedule>().WithMany().HasForeignKey("WorkScheduleId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("WorkScheduleId", "EmployeeId");
                    j.ToTable("WorkScheduleEmployees");
                    j.HasIndex("EmployeeId").HasDatabaseName("IX_WorkScheduleEmployees_EmployeeId");
                });

        // Private field access for collections
        builder.Navigation(ws => ws.Shifts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(ws => ws.Employees).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

