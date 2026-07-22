using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(u => u.CompanyId)
            .HasDatabaseName("IX_AspNetUsers_CompanyId");

        // Explicit relationship: ApplicationUser → Domain User (1:1 required)
        // Domain does NOT reference Identity types — the FK lives here only.
        builder.HasIndex(u => u.DomainUserId)
            .IsUnique()
            .HasDatabaseName("IX_AspNetUsers_DomainUserId");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.DomainUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

