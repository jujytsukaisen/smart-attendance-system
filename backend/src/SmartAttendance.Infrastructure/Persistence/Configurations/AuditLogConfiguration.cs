using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        // Primary Key
        builder.HasKey(al => al.Id);
        builder.Property(al => al.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(al => al.UserId);

        builder.Property(al => al.EntityName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(al => al.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Store old/new values as JSON (nvarchar(max) on SQL Server)
        builder.Property(al => al.OldValues)
            .IsRequired(false);

        builder.Property(al => al.NewValues)
            .IsRequired(false);

        builder.Property(al => al.IPAddress)
            .HasMaxLength(50);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(1000);

        // AuditLog's own CreatedAt (shadows BaseEntity.CreatedAt)
        builder.Property<DateTime>("CreatedAt")
            .IsRequired();

        builder.Property(al => al.CorrelationId)
            .HasMaxLength(100);

        // BaseEntity properties (AuditLog extends BaseEntity directly)
        builder.Property(al => al.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(al => al.UpdatedAt);

        // Indexes — optimized for audit trail queries
        builder.HasIndex(al => new { al.EntityName, al.EntityId })
            .HasDatabaseName("IX_AuditLogs_EntityName_EntityId");

        builder.HasIndex(al => al.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId")
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasIndex("CreatedAt")
            .HasDatabaseName("IX_AuditLogs_CreatedAt");

        builder.HasIndex(al => al.CorrelationId)
            .HasDatabaseName("IX_AuditLogs_CorrelationId")
            .HasFilter("[CorrelationId] IS NOT NULL");

        // NOTE: AuditLog intentionally has NO global query filter for soft delete
        // and NO tenant filter — audit records must be immutable and always accessible.
    }
}

