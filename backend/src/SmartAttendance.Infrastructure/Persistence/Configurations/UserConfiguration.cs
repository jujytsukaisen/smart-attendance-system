using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        // Value Object — Owned Entity
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);
        });

        builder.Property(u => u.UserStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Auditing
        builder.Property(u => u.CompanyId).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.CreatedBy).HasMaxLength(256);
        builder.Property(u => u.UpdatedBy).HasMaxLength(256);
        builder.Property(u => u.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(u => new { u.CompanyId, u.Username })
            .IsUnique()
            .HasDatabaseName("IX_Users_CompanyId_Username");

        builder.HasIndex(u => u.CompanyId)
            .HasDatabaseName("IX_Users_CompanyId");

        // Many-to-many: User ↔ Role (join table)
        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserRole",
                j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("UserId", "RoleId");
                    j.ToTable("UserRoles");
                    j.HasIndex("RoleId").HasDatabaseName("IX_UserRoles_RoleId");
                });

        // Private field access for roles collection
        builder.Navigation(u => u.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

