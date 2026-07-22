using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public sealed class AttendanceDeviceConfiguration : IEntityTypeConfiguration<AttendanceDevice>
{
    public void Configure(EntityTypeBuilder<AttendanceDevice> builder)
    {
        builder.ToTable("AttendanceDevices");

        // Primary Key
        builder.HasKey(ad => ad.Id);
        builder.Property(ad => ad.Id)
            .ValueGeneratedNever();

        // Identity
        builder.Property(ad => ad.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ad => ad.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ad => ad.SerialNumber)
            .HasMaxLength(200);

        // Device
        builder.Property(ad => ad.DeviceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ad => ad.DeviceStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ad => ad.OperatingSystem)
            .HasMaxLength(100);

        builder.Property(ad => ad.ApplicationVersion)
            .HasMaxLength(50);

        // Location
        builder.Property(ad => ad.Latitude).HasPrecision(9, 6);
        builder.Property(ad => ad.Longitude).HasPrecision(9, 6);
        builder.Property(ad => ad.AllowedRadiusMeters).HasPrecision(10, 2);

        // Connection
        builder.Property(ad => ad.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(ad => ad.Port);

        builder.Property(ad => ad.CommunicationProtocol)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(ad => ad.ConnectionStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Security
        builder.Property(ad => ad.PublicKey).HasMaxLength(4000);
        builder.Property(ad => ad.DeviceFingerprint).HasMaxLength(500);
        builder.Property(ad => ad.RequireDeviceApproval).IsRequired();
        builder.Property(ad => ad.RequireSecureConnection).IsRequired();

        // Capabilities
        builder.Property(ad => ad.SupportsFaceRecognition).IsRequired();
        builder.Property(ad => ad.SupportsFingerprint).IsRequired();
        builder.Property(ad => ad.SupportsGPS).IsRequired();
        builder.Property(ad => ad.SupportsOfflineAttendance).IsRequired();
        builder.Property(ad => ad.SupportsQRCode).IsRequired();
        builder.Property(ad => ad.SupportsNFC).IsRequired();

        // Configuration
        builder.Property(ad => ad.LastHeartbeat);
        builder.Property(ad => ad.LastSynchronization);

        builder.Property(ad => ad.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        // Auditing
        builder.Property(ad => ad.CompanyId).IsRequired();
        builder.Property(ad => ad.BranchId);
        builder.Property(ad => ad.CreatedAt).IsRequired();
        builder.Property(ad => ad.UpdatedAt);
        builder.Property(ad => ad.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(ad => ad.CreatedBy).HasMaxLength(256);
        builder.Property(ad => ad.UpdatedBy).HasMaxLength(256);
        builder.Property(ad => ad.DeletedBy).HasMaxLength(256);

        // Concurrency Token

        // Indexes
        builder.HasIndex(ad => new { ad.CompanyId, ad.Code })
            .IsUnique()
            .HasDatabaseName("IX_AttendanceDevices_CompanyId_Code");

        builder.HasIndex(ad => ad.CompanyId)
            .HasDatabaseName("IX_AttendanceDevices_CompanyId");

        builder.HasIndex(ad => ad.BranchId)
            .HasDatabaseName("IX_AttendanceDevices_BranchId")
            .HasFilter("[BranchId] IS NOT NULL");

        builder.HasIndex(ad => ad.DeviceStatus)
            .HasDatabaseName("IX_AttendanceDevices_DeviceStatus")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(ad => ad.SerialNumber)
            .HasDatabaseName("IX_AttendanceDevices_SerialNumber")
            .HasFilter("[SerialNumber] IS NOT NULL");

        // Relationships
        builder.HasOne(ad => ad.Branch)
            .WithMany(b => b.AttendanceDevices)
            .HasForeignKey(ad => ad.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

