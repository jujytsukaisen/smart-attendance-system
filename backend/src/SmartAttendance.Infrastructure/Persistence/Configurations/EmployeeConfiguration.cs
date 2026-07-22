using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        // Identity
        builder.Property(e => e.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.NationalId)
            .HasMaxLength(50);

        builder.Property(e => e.PassportNumber)
            .HasMaxLength(50);

        // Personal Information
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MiddleName)
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PreferredName)
            .HasMaxLength(100);

        builder.Property(e => e.Gender)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.DateOfBirth)
            .IsRequired();

        // Employment
        builder.Property(e => e.HireDate)
            .IsRequired();

        builder.Property(e => e.TerminationDate);

        builder.Property(e => e.EmployeeStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.JobTitle)
            .IsRequired()
            .HasMaxLength(200);

        // Relationship IDs
        builder.Property(e => e.CompanyId).IsRequired();
        builder.Property(e => e.BranchId).IsRequired();
        builder.Property(e => e.DepartmentId).IsRequired();
        builder.Property(e => e.AttendancePolicyId).IsRequired();
        builder.Property(e => e.UserId);
        builder.Property(e => e.FaceProfileId);
        builder.Property(e => e.ManagerId);

        // Value Objects — Owned Entities (Contact)
        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(x => x.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);
        });

        builder.OwnsOne(e => e.PhoneNumber, phone =>
        {
            phone.Property(x => x.Value)
                .HasColumnName("PhoneNumber")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.OwnsOne(e => e.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.City)
                .HasColumnName("Address_City")
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.State)
                .HasColumnName("Address_State")
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.Country)
                .HasColumnName("Address_Country")
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.ZipCode)
                .HasColumnName("Address_ZipCode")
                .IsRequired()
                .HasMaxLength(20);
        });

        // Emergency Contact
        builder.Property(e => e.EmergencyContactName)
            .HasMaxLength(200);

        builder.OwnsOne(e => e.EmergencyContactPhone, phone =>
        {
            phone.Property(x => x.Value)
                .HasColumnName("EmergencyContactPhone")
                .HasMaxLength(20);
        });

        builder.Property(e => e.EmergencyRelationship)
            .HasMaxLength(100);

        // Auditing
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(e => new { e.CompanyId, e.EmployeeNumber })
            .IsUnique()
            .HasDatabaseName("IX_Employees_CompanyId_EmployeeNumber");

        builder.HasIndex(e => e.CompanyId)
            .HasDatabaseName("IX_Employees_CompanyId");

        builder.HasIndex(e => e.BranchId)
            .HasDatabaseName("IX_Employees_BranchId");

        builder.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("IX_Employees_DepartmentId");

        builder.HasIndex(e => e.AttendancePolicyId)
            .HasDatabaseName("IX_Employees_AttendancePolicyId");

        builder.HasIndex(e => e.ManagerId)
            .HasDatabaseName("IX_Employees_ManagerId")
            .HasFilter("[ManagerId] IS NOT NULL");

        builder.HasIndex(e => e.EmployeeStatus)
            .HasDatabaseName("IX_Employees_Status")
            .HasFilter("[IsDeleted] = 0");

        // Relationships
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Employees)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AttendancePolicy)
            .WithMany(ap => ap.Employees)
            .HasForeignKey(e => e.AttendancePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.FaceProfile)
            .WithOne()
            .HasForeignKey<Employee>(e => e.FaceProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        // Self-referencing relationship (Manager → Subordinates)
        builder.HasOne(e => e.Manager)
            .WithMany(m => m.Subordinates)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Private field access for subordinates collection
        builder.Navigation(e => e.Subordinates).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

