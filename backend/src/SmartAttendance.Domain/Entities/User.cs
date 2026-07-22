using System;
using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.ValueObjects;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class User : BaseTenantEntity
{
    private readonly List<Role> _roles = new();

    public string Username { get; private set; }
    public Email Email { get; private set; }
    public UserStatus UserStatus { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    private User(
        Guid companyId,
        string username,
        Email email)
    {
        CompanyId = companyId;
        Username = username;
        Email = email;
        UserStatus = UserStatus.PendingActivation;
    }

    public static User Create(
        Guid companyId,
        string username,
        Email email)
    {
        if (companyId == Guid.Empty)
            throw new InvalidUserException("Company ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidUserException("Username cannot be empty.");
        if (email == null)
            throw new InvalidUserException("Email cannot be null.");

        return new User(companyId, username, email);
    }

    public void ChangeEmail(Email email)
    {
        if (email == null) throw new InvalidUserException("Email cannot be null.");
        Email = email;
    }

    public void ChangeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new InvalidUserException("Username cannot be empty.");
        Username = username;
    }

    public void Activate()
    {
        if (UserStatus == UserStatus.Active) return;
        UserStatus = UserStatus.Active;
    }

    public void Suspend()
    {
        if (UserStatus == UserStatus.Suspended) return;
        UserStatus = UserStatus.Suspended;
    }

    public void AssignRole(Role role)
    {
        if (role == null)
            throw new InvalidUserException("Role cannot be null.");

        if (_roles.Any(r => r.Id == role.Id))
            throw new InvalidUserException($"Role '{role.Name}' is already assigned to this user.");

        _roles.Add(role);
    }

    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new InvalidUserException("Role cannot be null.");

        var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);
        if (existingRole == null)
            return;

        if (UserStatus == UserStatus.Active && _roles.Count == 1)
            throw new InvalidUserException("Cannot remove the last role from an active user.");

        _roles.Remove(existingRole);
    }

    public void ReplaceRoles(IEnumerable<Role> roles)
    {
        if (roles == null)
            throw new InvalidUserException("Roles collection cannot be null.");

        var rolesList = roles.ToList();
        
        if (UserStatus == UserStatus.Active && rolesList.Count == 0)
            throw new InvalidUserException("Cannot replace roles with an empty collection for an active user.");

        _roles.Clear();

        foreach (var role in rolesList)
        {
            if (_roles.Any(r => r.Id == role.Id))
                throw new InvalidUserException($"Role '{role.Name}' is duplicated in the replacement collection.");

            _roles.Add(role);
        }
    }
}


