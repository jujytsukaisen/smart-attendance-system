using System;
using System.Collections.Generic;
using SmartAttendance.Domain.ValueObjects;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Company : BaseAuditableEntity
{
    private readonly List<Branch> _branches = new();
    private readonly List<Department> _departments = new();
    private readonly List<Employee> _employees = new();
    private readonly List<WorkSchedule> _workSchedules = new();
    private readonly List<Holiday> _holidays = new();

    public string Name { get; private set; }
    public string LegalName { get; private set; }
    public string RegistrationNumber { get; private set; }
    public string TaxNumber { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    public string? Website { get; private set; }
    public string? LogoUrl { get; private set; }
    public string TimeZone { get; private set; } = "UTC";
    public string Currency { get; private set; } = "USD";
    public bool IsActive { get; private set; }
    public DateTime FoundedDate { get; private set; }

    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();
    public IReadOnlyCollection<WorkSchedule> WorkSchedules => _workSchedules.AsReadOnly();
    public IReadOnlyCollection<Holiday> Holidays => _holidays.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Company() { }
#pragma warning restore CS8618

    private Company(
        string name,
        string legalName,
        string registrationNumber,
        string taxNumber,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        DateTime foundedDate,
        string? website = null,
        string? logoUrl = null)
    {
        Name = name;
        LegalName = legalName;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        FoundedDate = foundedDate;
        Website = website;
        LogoUrl = logoUrl;
        IsActive = true;
    }

    public static Company Create(
        string name,
        string legalName,
        string registrationNumber,
        string taxNumber,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        DateTime foundedDate,
        string? website = null,
        string? logoUrl = null)
    {
        ValidateRequiredFields(name, legalName, registrationNumber, taxNumber, email, phoneNumber, address, foundedDate);

        return new Company(
            name, 
            legalName, 
            registrationNumber, 
            taxNumber, 
            email, 
            phoneNumber, 
            address, 
            foundedDate, 
            website, 
            logoUrl);
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

    public void UpdateInformation(
        string name,
        string legalName,
        string registrationNumber,
        string taxNumber,
        DateTime foundedDate,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCompanyException("Company name cannot be empty.");
        if (string.IsNullOrWhiteSpace(legalName))
            throw new InvalidCompanyException("Legal name cannot be empty.");
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new InvalidCompanyException("Registration number cannot be empty.");
        if (string.IsNullOrWhiteSpace(taxNumber))
            throw new InvalidCompanyException("Tax number cannot be empty.");
        if (foundedDate > DateTime.UtcNow)
            throw new InvalidCompanyException("Founded date cannot be in the future.");

        Name = name;
        LegalName = legalName;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        FoundedDate = foundedDate;
        Website = website;
    }

    public void ChangeLogo(string? logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public void UpdateSettings(string timeZone, string currency)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new InvalidCompanyException("Time zone cannot be empty.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidCompanyException("Currency cannot be empty.");

        TimeZone = timeZone;
        Currency = currency;
    }

    public void ChangeContactInformation(Email email, PhoneNumber phoneNumber, Address address)
    {
        Email = email ?? throw new InvalidCompanyException("Email cannot be null.");
        PhoneNumber = phoneNumber ?? throw new InvalidCompanyException("Phone number cannot be null.");
        Address = address ?? throw new InvalidCompanyException("Address cannot be null.");
    }

    private static void ValidateRequiredFields(
        string name,
        string legalName,
        string registrationNumber,
        string taxNumber,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        DateTime foundedDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCompanyException("Company name cannot be empty.");
        if (string.IsNullOrWhiteSpace(legalName))
            throw new InvalidCompanyException("Legal name cannot be empty.");
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new InvalidCompanyException("Registration number cannot be empty.");
        if (string.IsNullOrWhiteSpace(taxNumber))
            throw new InvalidCompanyException("Tax number cannot be empty.");
        if (email == null)
            throw new InvalidCompanyException("Email cannot be null.");
        if (phoneNumber == null)
            throw new InvalidCompanyException("Phone number cannot be null.");
        if (address == null)
            throw new InvalidCompanyException("Address cannot be null.");
        if (foundedDate > DateTime.UtcNow)
            throw new InvalidCompanyException("Founded date cannot be in the future.");
    }
}
