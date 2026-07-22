using System;
using System.Collections.Generic;
using SmartAttendance.Domain.ValueObjects;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Branch : BaseTenantEntity
{
    private readonly List<Department> _departments = new();
    private readonly List<Employee> _employees = new();
    private readonly List<AttendanceDevice> _attendanceDevices = new();

    public string Name { get; private set; }
    public string Code { get; private set; }
    public Address Address { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Email Email { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double RadiusInMeters { get; private set; }
    public string TimeZone { get; private set; }
    public bool IsHeadOffice { get; private set; }
    public bool IsActive { get; private set; }

    // Relationships
    public Company Company { get; private set; } = null!;
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();
    public IReadOnlyCollection<AttendanceDevice> AttendanceDevices => _attendanceDevices.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Branch() { }
#pragma warning restore CS8618

    private Branch(
        Guid companyId,
        string name,
        string code,
        Address address,
        PhoneNumber phoneNumber,
        Email email,
        double latitude,
        double longitude,
        double radiusInMeters,
        string timeZone,
        bool isHeadOffice)
    {
        CompanyId = companyId;
        Name = name;
        Code = code;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        Latitude = latitude;
        Longitude = longitude;
        RadiusInMeters = radiusInMeters;
        TimeZone = timeZone;
        IsHeadOffice = isHeadOffice;
        IsActive = true;
    }

    public static Branch Create(
        Guid companyId,
        string name,
        string code,
        Address address,
        PhoneNumber phoneNumber,
        Email email,
        double latitude,
        double longitude,
        double radiusInMeters,
        string timeZone,
        bool isHeadOffice = false)
    {
        if (companyId == Guid.Empty)
            throw new InvalidBranchException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidBranchException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidBranchException("Code cannot be empty.");
        if (address == null)
            throw new InvalidBranchException("Address cannot be null.");
        if (phoneNumber == null)
            throw new InvalidBranchException("Phone number cannot be null.");
        if (email == null)
            throw new InvalidBranchException("Email cannot be null.");
        if (latitude < -90 || latitude > 90)
            throw new InvalidBranchException("Latitude must be between -90 and 90.");
        if (longitude < -180 || longitude > 180)
            throw new InvalidBranchException("Longitude must be between -180 and 180.");
        if (radiusInMeters <= 0)
            throw new InvalidBranchException("Radius must be greater than zero.");
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new InvalidBranchException("Time zone cannot be empty.");

        return new Branch(
            companyId,
            name,
            code,
            address,
            phoneNumber,
            email,
            latitude,
            longitude,
            radiusInMeters,
            timeZone,
            isHeadOffice);
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateInformation(string name, string code, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidBranchException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidBranchException("Code cannot be empty.");
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new InvalidBranchException("Time zone cannot be empty.");

        Name = name;
        Code = code;
        TimeZone = timeZone;
    }

    public void ChangeLocation(Address address, double latitude, double longitude)
    {
        if (address == null)
            throw new InvalidBranchException("Address cannot be null.");
        if (latitude < -90 || latitude > 90)
            throw new InvalidBranchException("Latitude must be between -90 and 90.");
        if (longitude < -180 || longitude > 180)
            throw new InvalidBranchException("Longitude must be between -180 and 180.");

        Address = address;
        Latitude = latitude;
        Longitude = longitude;
    }

    public void UpdateContactInformation(PhoneNumber phoneNumber, Email email)
    {
        if (phoneNumber == null)
            throw new InvalidBranchException("Phone number cannot be null.");
        if (email == null)
            throw new InvalidBranchException("Email cannot be null.");

        PhoneNumber = phoneNumber;
        Email = email;
    }

    public void ChangeRadius(double radiusInMeters)
    {
        if (radiusInMeters <= 0)
            throw new InvalidBranchException("Radius must be greater than zero.");

        RadiusInMeters = radiusInMeters;
    }

    public void MarkAsHeadOffice()
    {
        if (IsHeadOffice) return;
        IsHeadOffice = true;
    }

    public void RemoveHeadOffice()
    {
        if (!IsHeadOffice) return;
        IsHeadOffice = false;
    }
}

