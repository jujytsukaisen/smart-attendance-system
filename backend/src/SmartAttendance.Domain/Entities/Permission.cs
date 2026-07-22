using System;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Module { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystemPermission { get; private set; }

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Permission() { }
#pragma warning restore CS8618

    private Permission(
        string name,
        string code,
        string module,
        string? description,
        bool isSystemPermission)
    {
        Name = name;
        Code = code;
        Module = module;
        Description = description;
        IsSystemPermission = isSystemPermission;
    }

    public static Permission Create(
        string name,
        string code,
        string module,
        string? description = null,
        bool isSystemPermission = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPermissionException("Permission name cannot be empty.");
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidPermissionException("Permission code cannot be empty.");
        if (string.IsNullOrWhiteSpace(module))
            throw new InvalidPermissionException("Permission module cannot be empty.");

        return new Permission(name, code, module, description, isSystemPermission);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPermissionException("Permission name cannot be empty.");

        Name = name;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    public void MarkAsSystemPermission()
    {
        if (IsSystemPermission) return;
        IsSystemPermission = true;
    }

    public void Delete()
    {
        if (IsSystemPermission)
            throw new InvalidPermissionException("System permissions cannot be deleted.");

        IsDeleted = true;
    }
}

