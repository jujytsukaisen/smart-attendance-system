using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class FaceProfileConfiguration : IEntityTypeConfiguration<FaceProfile>
{
    public void Configure(EntityTypeBuilder<FaceProfile> builder)
    {
        builder.ToTable("FaceProfiles");

        // Primary Key
        builder.HasKey(fp => fp.Id);
        builder.Property(fp => fp.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(fp => fp.EmployeeId)
            .IsRequired();

        builder.Property(fp => fp.FaceProfileId)
            .HasMaxLength(500)
            .HasColumnName("ExternalFaceProfileId");

        builder.Property(fp => fp.RegistrationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(fp => fp.RegisteredAt);
        builder.Property(fp => fp.LastUpdatedAt);
        builder.Property(fp => fp.LastVerificationAt);

        builder.Property(fp => fp.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        // Auditing
        builder.Property(fp => fp.CompanyId).IsRequired();
        builder.Property(fp => fp.CreatedAt).IsRequired();
        builder.Property(fp => fp.UpdatedAt);
        builder.Property(fp => fp.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(fp => fp.CreatedBy).HasMaxLength(256);
        builder.Property(fp => fp.UpdatedBy).HasMaxLength(256);
        builder.Property(fp => fp.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(fp => fp.EmployeeId)
            .IsUnique()
            .HasDatabaseName("IX_FaceProfiles_EmployeeId");

        builder.HasIndex(fp => fp.CompanyId)
            .HasDatabaseName("IX_FaceProfiles_CompanyId");

        // Relationships — one-to-one with Employee (configured from Employee side too)
        builder.HasOne<Employee>()
            .WithOne(e => e.FaceProfile)
            .HasForeignKey<FaceProfile>(fp => fp.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

