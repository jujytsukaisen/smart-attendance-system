using System;
using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Role : BaseTenantEntity
{
    private readonly List<Permission> _permissions = new();

    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Role() { }
#pragma warning restore CS8618

    private Role(
        Guid companyId,
        string name,
        string code,
        string? description,
        bool isSystemRole)
    {
        CompanyId = companyId;
        Name = name;
        Code = code;
        Description = description;
        IsSystemRole = isSystemRole;
        IsActive = false; // Roles must have at least one permission before activation
    }

    public static Role Create(
        Guid companyId,
        string name,
        string code,
        string? description = null,
        bool isSystemRole = false)
    {
        if (companyId == Guid.Empty)
            throw new InvalidRoleException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidRoleException("Role name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidRoleException("Role code cannot be empty.");

        return new Role(companyId, name, code, description, isSystemRole);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidRoleException("Role name cannot be empty.");

        Name = name;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    public void Activate()
    {
        if (IsActive) return;

        if (_permissions.Count == 0)
            throw new InvalidRoleException("At least one permission must exist before activating a role.");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new InvalidRoleException("Permission cannot be null.");

        if (_permissions.Any(p => p.Id == permission.Id))
            throw new InvalidRoleException($"Permission '{permission.Name}' is already added to this role.");

        _permissions.Add(permission);
    }

    public void RemovePermission(Permission permission)
    {
        if (permission == null)
            throw new InvalidRoleException("Permission cannot be null.");

        var existingPermission = _permissions.FirstOrDefault(p => p.Id == permission.Id);
        if (existingPermission == null)
            return;

        if (IsActive && _permissions.Count == 1)
            throw new InvalidRoleException("Removing the last permission from an active role is not allowed.");

        _permissions.Remove(existingPermission);
    }

    public void ReplacePermissions(IEnumerable<Permission> permissions)
    {
        if (permissions == null)
            throw new InvalidRoleException("Permissions collection cannot be null.");

        var permissionsList = permissions.ToList();
        
        if (IsActive && permissionsList.Count == 0)
            throw new InvalidRoleException("Cannot replace permissions with an empty collection for an active role.");

        _permissions.Clear();

        foreach (var permission in permissionsList)
        {
            if (_permissions.Any(p => p.Id == permission.Id))
                throw new InvalidRoleException($"Permission '{permission.Name}' is duplicated in the replacement collection.");

            _permissions.Add(permission);
        }
    }

    public void MarkAsSystemRole()
    {
        if (IsSystemRole) return;
        IsSystemRole = true;
    }

    public void Delete()
    {
        if (IsSystemRole)
            throw new InvalidRoleException("System roles cannot be deleted.");

        IsDeleted = true;
    }
}

