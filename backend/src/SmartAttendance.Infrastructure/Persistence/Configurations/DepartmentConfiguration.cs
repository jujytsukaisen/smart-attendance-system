using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        // Primary Key
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        // Scalar Properties
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.ManagerId);

        builder.Property(d => d.BranchId)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Auditing
        builder.Property(d => d.CompanyId).IsRequired();
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);
        builder.Property(d => d.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(d => d.CreatedBy).HasMaxLength(256);
        builder.Property(d => d.UpdatedBy).HasMaxLength(256);
        builder.Property(d => d.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(d => new { d.CompanyId, d.BranchId, d.Code })
            .IsUnique()
            .HasDatabaseName("IX_Departments_CompanyId_BranchId_Code");

        builder.HasIndex(d => d.CompanyId)
            .HasDatabaseName("IX_Departments_CompanyId");

        builder.HasIndex(d => d.BranchId)
            .HasDatabaseName("IX_Departments_BranchId");

        builder.HasIndex(d => d.ManagerId)
            .HasDatabaseName("IX_Departments_ManagerId")
            .HasFilter("[ManagerId] IS NOT NULL");

        // Relationships
        builder.HasOne(d => d.Branch)
            .WithMany(b => b.Departments)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Employees)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Private field access for collections
        builder.Navigation(d => d.Employees).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

