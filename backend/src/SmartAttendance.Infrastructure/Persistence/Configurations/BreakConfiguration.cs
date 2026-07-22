using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class BreakConfiguration : IEntityTypeConfiguration<Break>
{
    public void Configure(EntityTypeBuilder<Break> builder)
    {
        builder.ToTable("Breaks");

        // Primary Key
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.StartTime)
            .IsRequired();

        builder.Property(b => b.EndTime)
            .IsRequired();

        builder.Property(b => b.DurationMinutes)
            .IsRequired();

        builder.Property(b => b.BreakType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.IsPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.IsMandatory)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.ShiftId)
            .IsRequired();

        // Auditing
        builder.Property(b => b.CompanyId).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt);
        builder.Property(b => b.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.CreatedBy).HasMaxLength(256);
        builder.Property(b => b.UpdatedBy).HasMaxLength(256);
        builder.Property(b => b.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(b => b.ShiftId)
            .HasDatabaseName("IX_Breaks_ShiftId");

        builder.HasIndex(b => b.CompanyId)
            .HasDatabaseName("IX_Breaks_CompanyId");

        // Relationships
        builder.HasOne(b => b.Shift)
            .WithMany(s => s.Breaks)
            .HasForeignKey(b => b.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

