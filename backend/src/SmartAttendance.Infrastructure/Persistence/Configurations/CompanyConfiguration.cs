using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        // Primary Key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        // Scalar Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.TaxNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Website)
            .HasMaxLength(500);

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.FoundedDate)
            .IsRequired();

        // Value Objects — Owned Entities
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);
        });

        builder.OwnsOne(c => c.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.OwnsOne(c => c.Address, address =>
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

        // Auditing (BaseAuditableEntity)
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(c => c.CreatedBy).HasMaxLength(256);
        builder.Property(c => c.UpdatedBy).HasMaxLength(256);
        builder.Property(c => c.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(c => c.RegistrationNumber)
            .IsUnique()
            .HasDatabaseName("IX_Companies_RegistrationNumber");

        builder.HasIndex(c => c.TaxNumber)
            .IsUnique()
            .HasDatabaseName("IX_Companies_TaxNumber");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Companies_IsActive")
            .HasFilter("[IsDeleted] = 0");

        // Collections — backing fields
        builder.HasMany(c => c.Branches)
            .WithOne(b => b.Company)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Departments)
            .WithOne()
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Employees)
            .WithOne(e => e.Company)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.WorkSchedules)
            .WithOne()
            .HasForeignKey(ws => ws.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Holidays)
            .WithOne()
            .HasForeignKey(h => h.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Private field access for collections
        builder.Navigation(c => c.Branches).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Departments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Employees).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.WorkSchedules).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Holidays).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

