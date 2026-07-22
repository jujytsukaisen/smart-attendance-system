using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        // Primary Key
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        // Scalar Properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Latitude)
            .HasPrecision(9, 6);

        builder.Property(b => b.Longitude)
            .HasPrecision(9, 6);

        builder.Property(b => b.RadiusInMeters)
            .HasPrecision(10, 2);

        builder.Property(b => b.TimeZone)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.IsHeadOffice)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Value Objects — Owned Entities
        builder.OwnsOne(b => b.Address, address =>
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

        builder.OwnsOne(b => b.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.OwnsOne(b => b.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);
        });

        // Auditing (BaseTenantEntity → BaseAuditableEntity)
        builder.Property(b => b.CompanyId).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt);
        builder.Property(b => b.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.CreatedBy).HasMaxLength(256);
        builder.Property(b => b.UpdatedBy).HasMaxLength(256);
        builder.Property(b => b.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(b => new { b.CompanyId, b.Code })
            .IsUnique()
            .HasDatabaseName("IX_Branches_CompanyId_Code");

        builder.HasIndex(b => b.CompanyId)
            .HasDatabaseName("IX_Branches_CompanyId");

        builder.HasIndex(b => new { b.CompanyId, b.IsHeadOffice })
            .HasDatabaseName("IX_Branches_CompanyId_IsHeadOffice")
            .HasFilter("[IsHeadOffice] = 1 AND [IsDeleted] = 0");

        // Relationships
        builder.HasOne(b => b.Company)
            .WithMany(c => c.Branches)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Departments)
            .WithOne(d => d.Branch)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Employees)
            .WithOne(e => e.Branch)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.AttendanceDevices)
            .WithOne(ad => ad.Branch)
            .HasForeignKey(ad => ad.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Private field access for collections
        builder.Navigation(b => b.Departments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(b => b.Employees).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(b => b.AttendanceDevices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

