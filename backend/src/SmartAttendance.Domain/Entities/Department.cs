using System;
using System.Collections.Generic;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Department : BaseTenantEntity
{
    private readonly List<Employee> _employees = new();

    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public Guid? ManagerId { get; private set; }
    public Guid BranchId { get; private set; }
    public bool IsActive { get; private set; }

    // Relationships
    public Branch Branch { get; private set; } = null!;
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Department() { }
#pragma warning restore CS8618

    private Department(
        Guid companyId,
        Guid branchId,
        string name,
        string code,
        string? description)
    {
        CompanyId = companyId;
        BranchId = branchId;
        Name = name;
        Code = code;
        Description = description;
        IsActive = true;
    }

    public static Department Create(
        Guid companyId,
        Guid branchId,
        string name,
        string code,
        string? description = null)
    {
        if (companyId == Guid.Empty)
            throw new InvalidDepartmentException("Company ID cannot be empty.");
        if (branchId == Guid.Empty)
            throw new InvalidDepartmentException("Branch ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidDepartmentException("Department name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidDepartmentException("Department code cannot be empty.");

        return new Department(companyId, branchId, name, code, description);
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

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidDepartmentException("Department name cannot be empty.");

        Name = name;
    }

    public void ChangeBranch(Guid branchId)
    {
        if (branchId == Guid.Empty)
            throw new InvalidDepartmentException("Branch ID cannot be empty.");

        BranchId = branchId;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    public void AssignManager(Guid managerId)
    {
        if (managerId == Guid.Empty)
            throw new InvalidDepartmentException("Manager ID cannot be empty.");
        
        ManagerId = managerId;
    }

    public void RemoveManager()
    {
        ManagerId = null;
    }

    public void Delete()
    {
        if (_employees.Count > 0)
            throw new InvalidDepartmentException("A department cannot be deleted while it still contains employees.");

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}


