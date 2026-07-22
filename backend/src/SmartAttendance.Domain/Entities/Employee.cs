using System;
using System.Collections.Generic;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.ValueObjects;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Employee : BaseTenantEntity
{
    private readonly List<Employee> _subordinates = new();

    // Identity
    public string EmployeeNumber { get; private set; }
    public string? NationalId { get; private set; }
    public string? PassportNumber { get; private set; }

    // Personal Information
    public string FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string? PreferredName { get; private set; }
    public Gender Gender { get; private set; }
    public DateTime DateOfBirth { get; private set; }

    // Employment
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public EmployeeStatus EmployeeStatus { get; private set; }
    public string JobTitle { get; private set; }

    // Relationship IDs
    public Guid BranchId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid AttendancePolicyId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? FaceProfileId { get; private set; }
    public Guid? ManagerId { get; private set; }

    // Relationships Navigation
    public Company Company { get; private set; } = null!;
    public Branch Branch { get; private set; } = null!;
    public Department Department { get; private set; } = null!;
    public AttendancePolicy AttendancePolicy { get; private set; } = null!;
    public User? User { get; private set; }
    public FaceProfile? FaceProfile { get; private set; }
    public Employee? Manager { get; private set; }
    public IReadOnlyCollection<Employee> Subordinates => _subordinates.AsReadOnly();

    // Contact
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }

    // Emergency Contact
    public string? EmergencyContactName { get; private set; }
    public PhoneNumber? EmergencyContactPhone { get; private set; }
    public string? EmergencyRelationship { get; private set; }

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Employee() { }
#pragma warning restore CS8618

    private Employee(
        Guid companyId,
        string employeeNumber,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        DateTime hireDate,
        string jobTitle,
        Guid branchId,
        Guid departmentId,
        Guid attendancePolicyId,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        string? middleName,
        string? preferredName,
        string? nationalId,
        string? passportNumber)
    {
        CompanyId = companyId;
        EmployeeNumber = employeeNumber;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        HireDate = hireDate;
        JobTitle = jobTitle;
        BranchId = branchId;
        DepartmentId = departmentId;
        AttendancePolicyId = attendancePolicyId;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        
        MiddleName = middleName;
        PreferredName = preferredName;
        NationalId = nationalId;
        PassportNumber = passportNumber;

        EmployeeStatus = EmployeeStatus.Active;
    }

    public static Employee Create(
        Guid companyId,
        string employeeNumber,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        DateTime hireDate,
        string jobTitle,
        Guid branchId,
        Guid departmentId,
        Guid attendancePolicyId,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        string? middleName = null,
        string? preferredName = null,
        string? nationalId = null,
        string? passportNumber = null)
    {
        if (companyId == Guid.Empty)
            throw new InvalidEmployeeException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new InvalidEmployeeException("Employee number cannot be empty.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidEmployeeException("First name cannot be empty.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidEmployeeException("Last name cannot be empty.");
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new InvalidEmployeeException("Job title cannot be empty.");
        if (branchId == Guid.Empty)
            throw new InvalidEmployeeException("Branch ID cannot be empty.");
        if (departmentId == Guid.Empty)
            throw new InvalidEmployeeException("Department ID cannot be empty.");
        if (attendancePolicyId == Guid.Empty)
            throw new InvalidEmployeeException("Attendance policy ID cannot be empty.");
        if (email == null)
            throw new InvalidEmployeeException("Email cannot be null.");
        if (phoneNumber == null)
            throw new InvalidEmployeeException("Phone number cannot be null.");
        if (address == null)
            throw new InvalidEmployeeException("Address cannot be null.");

        if (hireDate > DateTime.UtcNow)
            throw new InvalidEmployeeException("Hire date cannot be in the future.");

        return new Employee(
            companyId, 
            employeeNumber, 
            firstName, 
            lastName, 
            dateOfBirth, 
            gender, 
            hireDate, 
            jobTitle, 
            branchId, 
            departmentId, 
            attendancePolicyId,
            email, 
            phoneNumber, 
            address, 
            middleName, 
            preferredName, 
            nationalId, 
            passportNumber);
    }

    public void Activate()
    {
        if (EmployeeStatus == EmployeeStatus.Terminated)
            throw new InvalidEmployeeException("Cannot activate a terminated employee.");

        if (EmployeeStatus == EmployeeStatus.Active) return;
        
        EmployeeStatus = EmployeeStatus.Active;
    }

    public void Deactivate(bool isAdmin = false)
    {
        if (EmployeeStatus == EmployeeStatus.Terminated)
            throw new InvalidEmployeeException("Cannot deactivate a terminated employee.");

        if (isAdmin && EmployeeStatus == EmployeeStatus.Active)
            throw new InvalidEmployeeException("Cannot deactivate an active admin.");

        if (EmployeeStatus == EmployeeStatus.Inactive) return;
        
        EmployeeStatus = EmployeeStatus.Inactive;
    }

    public void Terminate(DateTime terminationDate)
    {
        if (EmployeeStatus == EmployeeStatus.Terminated)
            throw new InvalidEmployeeException("Employee is already terminated. Cannot terminate twice.");

        if (terminationDate < HireDate)
            throw new InvalidEmployeeException("Termination date cannot be before hire date.");

        EmployeeStatus = EmployeeStatus.Terminated;
        TerminationDate = terminationDate;
        
        // Remove manager relationship when terminated
        ManagerId = null;
    }

    public void TransferToDepartment(Guid departmentId)
    {
        if (EmployeeStatus == EmployeeStatus.Terminated)
            throw new InvalidEmployeeException("Cannot transfer a terminated employee.");

        if (departmentId == Guid.Empty)
            throw new InvalidEmployeeException("Department ID cannot be empty.");

        if (departmentId == DepartmentId)
            throw new InvalidEmployeeException("Cannot assign duplicate department.");
        
        DepartmentId = departmentId;
    }

    public void TransferToBranch(Guid branchId)
    {
        if (EmployeeStatus == EmployeeStatus.Terminated)
            throw new InvalidEmployeeException("Cannot transfer a terminated employee.");

        if (branchId == Guid.Empty)
            throw new InvalidEmployeeException("Branch ID cannot be empty.");

        BranchId = branchId;
    }

    public void AssignManager(Guid managerId)
    {
        if (EmployeeStatus == EmployeeStatus.Terminated)
            throw new InvalidEmployeeException("Cannot assign a manager to a terminated employee.");

        if (managerId == Guid.Empty)
            throw new InvalidEmployeeException("Manager ID cannot be empty.");

        if (managerId == Id && Id != Guid.Empty)
            throw new InvalidEmployeeException("Manager cannot be self.");

        ManagerId = managerId;
    }

    public void RemoveManager()
    {
        ManagerId = null;
    }

    public void AssignAttendancePolicy(Guid attendancePolicyId)
    {
        if (attendancePolicyId == Guid.Empty)
            throw new InvalidEmployeeException("Attendance policy ID cannot be empty.");

        AttendancePolicyId = attendancePolicyId;
    }

    public void AssignFaceProfile(Guid faceProfileId)
    {
        if (faceProfileId == Guid.Empty)
            throw new InvalidEmployeeException("Face profile ID cannot be empty.");

        FaceProfileId = faceProfileId;
    }

    public void UpdatePersonalInformation(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        string? middleName = null,
        string? preferredName = null,
        string? nationalId = null,
        string? passportNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidEmployeeException("First name cannot be empty.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidEmployeeException("Last name cannot be empty.");

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        MiddleName = middleName;
        PreferredName = preferredName;
        NationalId = nationalId;
        PassportNumber = passportNumber;
    }

    public void UpdateContactInformation(Email email, PhoneNumber phoneNumber, Address address)
    {
        Email = email ?? throw new InvalidEmployeeException("Email cannot be null.");
        PhoneNumber = phoneNumber ?? throw new InvalidEmployeeException("Phone number cannot be null.");
        Address = address ?? throw new InvalidEmployeeException("Address cannot be null.");
    }

    public void UpdateEmergencyContact(string? name, PhoneNumber? phone, string? relationship)
    {
        EmergencyContactName = name;
        EmergencyContactPhone = phone;
        EmergencyRelationship = relationship;
    }

    public void ChangeJobTitle(string jobTitle)
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new InvalidEmployeeException("Job title cannot be empty.");

        JobTitle = jobTitle;
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        EmployeeStatus = EmployeeStatus.Inactive;
        // Optionally, if they are deleted, terminate them
        if (EmployeeStatus != EmployeeStatus.Terminated)
        {
            EmployeeStatus = EmployeeStatus.Terminated;
            TerminationDate = DateTime.UtcNow;
            ManagerId = null;
        }
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        // Upon restore, usually they would be active or inactive. Default to active.
        EmployeeStatus = EmployeeStatus.Active;
        TerminationDate = null;
    }
}

