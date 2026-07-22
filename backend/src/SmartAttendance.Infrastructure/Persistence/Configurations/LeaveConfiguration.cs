using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class LeaveConfiguration : IEntityTypeConfiguration<Leave>
{
    public void Configure(EntityTypeBuilder<Leave> builder)
    {
        builder.ToTable("Leaves");

        // Primary Key
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(l => l.EmployeeId)
            .IsRequired();

        builder.Property(l => l.LeaveType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.LeaveStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.StartDate)
            .IsRequired();

        builder.Property(l => l.EndDate)
            .IsRequired();

        builder.Property(l => l.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.Remarks)
            .HasMaxLength(2000);

        builder.Property(l => l.RequestedAt)
            .IsRequired();

        builder.Property(l => l.ApprovedAt);
        builder.Property(l => l.RejectedAt);
        builder.Property(l => l.CancelledAt);
        builder.Property(l => l.ApprovedBy);
        builder.Property(l => l.RejectedBy);
        builder.Property(l => l.CancelledBy);

        builder.Property(l => l.TotalDays)
            .IsRequired()
            .HasPrecision(5, 1);

        // Auditing
        builder.Property(l => l.CompanyId).IsRequired();
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt);
        builder.Property(l => l.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(l => l.CreatedBy).HasMaxLength(256);
        builder.Property(l => l.UpdatedBy).HasMaxLength(256);
        builder.Property(l => l.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes — optimized for leave queries
        builder.HasIndex(l => new { l.EmployeeId, l.StartDate, l.EndDate })
            .HasDatabaseName("IX_Leaves_EmployeeId_DateRange");

        builder.HasIndex(l => new { l.CompanyId, l.LeaveStatus })
            .HasDatabaseName("IX_Leaves_CompanyId_Status")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(l => l.CompanyId)
            .HasDatabaseName("IX_Leaves_CompanyId");

        builder.HasIndex(l => l.EmployeeId)
            .HasDatabaseName("IX_Leaves_EmployeeId");

        // Relationships
        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

