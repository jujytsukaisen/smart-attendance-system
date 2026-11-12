using System;
using SmartAttendance.Domain.Enums;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.AttendanceDevices.DTOs;

public class AttendanceDeviceDetailsDto : IMapFrom<AttendanceDevice>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    
    // Identity
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }

    // Device
    public DeviceType DeviceType { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ApplicationVersion { get; set; }

    // Connection
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public CommunicationProtocol CommunicationProtocol { get; set; }
    public ConnectionStatus ConnectionStatus { get; set; }

    // Location
    public Guid? BranchId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AllowedRadiusMeters { get; set; }

    // Security
    public string? PublicKey { get; set; }
    public string? DeviceFingerprint { get; set; }
    public bool RequireDeviceApproval { get; set; }
    public bool RequireSecureConnection { get; set; }

    // Capabilities
    public bool SupportsFaceRecognition { get; set; }
    public bool SupportsFingerprint { get; set; }
    public bool SupportsGPS { get; set; }
    public bool SupportsOfflineAttendance { get; set; }
    public bool SupportsQRCode { get; set; }
    public bool SupportsNFC { get; set; }

    // Configuration
    public DateTime? LastHeartbeat { get; set; }
    public DateTime? LastSynchronization { get; set; }
    public bool IsActive { get; set; }
}
