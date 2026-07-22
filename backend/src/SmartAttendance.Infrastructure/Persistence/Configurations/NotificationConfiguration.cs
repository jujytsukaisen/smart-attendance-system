using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        // Primary Key
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(n => n.RecipientUserId)
            .IsRequired();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(n => n.NotificationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Notification's own CreatedAt (shadows BaseEntity.CreatedAt)
        builder.Property<DateTime>("CreatedAt")
            .IsRequired();

        builder.Property(n => n.ReadAt);
        builder.Property(n => n.SentAt);

        // Auditing (from BaseTenantEntity → BaseAuditableEntity → BaseEntity)
        builder.Property(n => n.CompanyId).IsRequired();
        builder.Property(n => n.UpdatedAt);
        builder.Property(n => n.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(n => n.CreatedBy).HasMaxLength(256);
        builder.Property(n => n.UpdatedBy).HasMaxLength(256);
        builder.Property(n => n.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes — optimized for notification inbox queries
        builder.HasIndex(n => new { n.RecipientUserId, n.Status })
            .HasDatabaseName("IX_Notifications_RecipientUserId_Status")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(n => n.CompanyId)
            .HasDatabaseName("IX_Notifications_CompanyId");

        builder.HasIndex(n => n.RecipientUserId)
            .HasDatabaseName("IX_Notifications_RecipientUserId");

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

