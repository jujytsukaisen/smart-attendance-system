using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class AttendanceDevice : BaseTenantEntity
{
    // Identity
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? SerialNumber { get; private set; }

    // Device
    public DeviceType DeviceType { get; private set; }
    public DeviceStatus DeviceStatus { get; private set; }
    public string? OperatingSystem { get; private set; }
    public string? ApplicationVersion { get; private set; }

    // Location
    public Guid? BranchId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double AllowedRadiusMeters { get; private set; }

    // Security
    public string? PublicKey { get; private set; }
    public string? DeviceFingerprint { get; private set; }
    public bool RequireDeviceApproval { get; private set; }
    public bool RequireSecureConnection { get; private set; }

    // Capabilities
    public bool SupportsFaceRecognition { get; private set; }
    public bool SupportsFingerprint { get; private set; }
    public bool SupportsGPS { get; private set; }
    public bool SupportsOfflineAttendance { get; private set; }
    public bool SupportsQRCode { get; private set; }
    public bool SupportsNFC { get; private set; }

    // Configuration
    public DateTime? LastHeartbeat { get; private set; }
    public DateTime? LastSynchronization { get; private set; }
    public bool IsActive { get; private set; }

    // Connection
    public string? IpAddress { get; private set; }
    public int? Port { get; private set; }
    public CommunicationProtocol CommunicationProtocol { get; private set; }
    public ConnectionStatus ConnectionStatus { get; private set; }

    // Relationships
    public Branch? Branch { get; private set; }

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private AttendanceDevice() { }
#pragma warning restore CS8618

    private AttendanceDevice(
        Guid companyId,
        string name,
        string code,
        string? serialNumber,
        DeviceType deviceType,
        string? operatingSystem,
        string? applicationVersion,
        Guid? branchId,
        double latitude,
        double longitude,
        double allowedRadiusMeters,
        string? publicKey,
        string? deviceFingerprint,
        bool requireDeviceApproval,
        bool requireSecureConnection,
        bool supportsFaceRecognition,
        bool supportsFingerprint,
        bool supportsGPS,
        bool supportsOfflineAttendance,
        bool supportsQRCode,
        bool supportsNFC,
        string? ipAddress,
        int? port,
        CommunicationProtocol communicationProtocol)
    {
        CompanyId = companyId;
        Name = name;
        Code = code;
        SerialNumber = serialNumber;
        DeviceType = deviceType;
        OperatingSystem = operatingSystem;
        ApplicationVersion = applicationVersion;
        BranchId = branchId;
        Latitude = latitude;
        Longitude = longitude;
        AllowedRadiusMeters = allowedRadiusMeters;
        PublicKey = publicKey;
        DeviceFingerprint = deviceFingerprint;
        RequireDeviceApproval = requireDeviceApproval;
        RequireSecureConnection = requireSecureConnection;
        SupportsFaceRecognition = supportsFaceRecognition;
        SupportsFingerprint = supportsFingerprint;
        SupportsGPS = supportsGPS;
        SupportsOfflineAttendance = supportsOfflineAttendance;
        SupportsQRCode = supportsQRCode;
        SupportsNFC = supportsNFC;
        IpAddress = ipAddress;
        Port = port;
        CommunicationProtocol = communicationProtocol;

        IsActive = false; // Devices must be explicitly activated
        ConnectionStatus = ConnectionStatus.Offline; // Initially offline
        
        // Logical rule: if approval is required, start as pending
        DeviceStatus = requireDeviceApproval ? DeviceStatus.PendingApproval : DeviceStatus.Approved;
    }

    public static AttendanceDevice Create(
        Guid companyId,
        string name,
        string code,
        string? serialNumber,
        DeviceType deviceType,
        string? operatingSystem,
        string? applicationVersion,
        Guid? branchId,
        double latitude,
        double longitude,
        double allowedRadiusMeters,
        string? publicKey,
        string? deviceFingerprint,
        bool requireDeviceApproval,
        bool requireSecureConnection,
        bool supportsFaceRecognition,
        bool supportsFingerprint,
        bool supportsGPS,
        bool supportsOfflineAttendance,
        bool supportsQRCode,
        bool supportsNFC,
        string? ipAddress = null,
        int? port = null,
        CommunicationProtocol communicationProtocol = CommunicationProtocol.TCP)
    {
        if (companyId == Guid.Empty)
            throw new InvalidAttendanceDeviceException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidAttendanceDeviceException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidAttendanceDeviceException("Code cannot be empty.");
        if (allowedRadiusMeters < 0)
            throw new InvalidAttendanceDeviceException("Radius cannot be negative.");
        if (port.HasValue && (port < 1 || port > 65535))
            throw new InvalidAttendanceDeviceException("Port must be between 1 and 65535.");

        ValidateCoordinates(latitude, longitude);

        return new AttendanceDevice(
            companyId,
            name,
            code,
            serialNumber,
            deviceType,
            operatingSystem,
            applicationVersion,
            branchId,
            latitude,
            longitude,
            allowedRadiusMeters,
            publicKey,
            deviceFingerprint,
            requireDeviceApproval,
            requireSecureConnection,
            supportsFaceRecognition,
            supportsFingerprint,
            supportsGPS,
            supportsOfflineAttendance,
            supportsQRCode,
            supportsNFC,
            ipAddress,
            port,
            communicationProtocol);
    }

    public void Update(
        string name,
        string code,
        string? serialNumber,
        DeviceType deviceType,
        string? operatingSystem,
        string? applicationVersion,
        Guid? branchId,
        double latitude,
        double longitude,
        double allowedRadiusMeters,
        string? publicKey,
        string? deviceFingerprint,
        bool requireDeviceApproval,
        bool requireSecureConnection,
        bool supportsFaceRecognition,
        bool supportsFingerprint,
        bool supportsGPS,
        bool supportsOfflineAttendance,
        bool supportsQRCode,
        bool supportsNFC,
        string? ipAddress,
        int? port,
        CommunicationProtocol communicationProtocol)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidAttendanceDeviceException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidAttendanceDeviceException("Code cannot be empty.");
        if (allowedRadiusMeters < 0)
            throw new InvalidAttendanceDeviceException("Radius cannot be negative.");
        if (port.HasValue && (port < 1 || port > 65535))
            throw new InvalidAttendanceDeviceException("Port must be between 1 and 65535.");

        ValidateCoordinates(latitude, longitude);

        Name = name;
        Code = code;
        SerialNumber = serialNumber;
        DeviceType = deviceType;
        OperatingSystem = operatingSystem;
        ApplicationVersion = applicationVersion;
        BranchId = branchId;
        Latitude = latitude;
        Longitude = longitude;
        AllowedRadiusMeters = allowedRadiusMeters;
        PublicKey = publicKey;
        DeviceFingerprint = deviceFingerprint;
        
        // Handle approval logic change
        if (requireDeviceApproval && !RequireDeviceApproval)
        {
            DeviceStatus = DeviceStatus.PendingApproval; // Re-requires approval if changed
            IsActive = false; // Must be inactive until approved
        }
        
        RequireDeviceApproval = requireDeviceApproval;
        RequireSecureConnection = requireSecureConnection;
        SupportsFaceRecognition = supportsFaceRecognition;
        SupportsFingerprint = supportsFingerprint;
        SupportsGPS = supportsGPS;
        SupportsOfflineAttendance = supportsOfflineAttendance;
        SupportsQRCode = supportsQRCode;
        SupportsNFC = supportsNFC;
        IpAddress = ipAddress;
        Port = port;
        CommunicationProtocol = communicationProtocol;
    }

    public void Activate()
    {
        if (IsActive) return;
        if (RequireDeviceApproval && DeviceStatus != DeviceStatus.Approved)
            throw new InvalidAttendanceDeviceException("Device must be approved before activation.");
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void Approve()
    {
        if (!RequireDeviceApproval)
            throw new InvalidAttendanceDeviceException("This device does not require approval.");

        if (DeviceStatus == DeviceStatus.Approved) return;

        DeviceStatus = DeviceStatus.Approved;
    }

    public void Revoke()
    {
        if (DeviceStatus == DeviceStatus.Revoked) return;
        
        DeviceStatus = DeviceStatus.Revoked;
        IsActive = false; // Logical integration: Revoked devices should not be active
        ConnectionStatus = ConnectionStatus.Offline;
    }

    public void UpdateLocation(double latitude, double longitude, double allowedRadiusMeters)
    {
        if (allowedRadiusMeters < 0)
            throw new InvalidAttendanceDeviceException("Radius cannot be negative.");

        ValidateCoordinates(latitude, longitude);

        Latitude = latitude;
        Longitude = longitude;
        AllowedRadiusMeters = allowedRadiusMeters;
    }

    public void UpdateCapabilities(
        bool supportsFaceRecognition,
        bool supportsFingerprint,
        bool supportsGPS,
        bool supportsOfflineAttendance,
        bool supportsQRCode,
        bool supportsNFC)
    {
        SupportsFaceRecognition = supportsFaceRecognition;
        SupportsFingerprint = supportsFingerprint;
        SupportsGPS = supportsGPS;
        SupportsOfflineAttendance = supportsOfflineAttendance;
        SupportsQRCode = supportsQRCode;
        SupportsNFC = supportsNFC;
    }

    public void RecordHeartbeat()
    {
        if (DeviceStatus == DeviceStatus.Revoked)
            throw new InvalidAttendanceDeviceException("Device cannot record heartbeat while revoked.");

        LastHeartbeat = DateTimeOffset.UtcNow.UtcDateTime;
        ConnectionStatus = ConnectionStatus.Online;
    }

    public void RecordSynchronization()
    {
        if (DeviceStatus == DeviceStatus.Revoked)
            throw new InvalidAttendanceDeviceException("Device cannot record synchronization while revoked.");

        LastSynchronization = DateTimeOffset.UtcNow.UtcDateTime;
        ConnectionStatus = ConnectionStatus.Online;
    }

    public void UpdateConnectionStatus(ConnectionStatus status)
    {
        ConnectionStatus = status;
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new InvalidAttendanceDeviceException("Latitude must be between -90 and 90 degrees.");
        if (longitude < -180 || longitude > 180)
            throw new InvalidAttendanceDeviceException("Longitude must be between -180 and 180 degrees.");
    }
}

