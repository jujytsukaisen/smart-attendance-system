using System;
using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(Guid domainUserId, string userName, string password, string? email = null);
    Task<Result> DeleteUserAsync(string userId);
    Task<Result> DeleteUserByDomainIdAsync(Guid domainUserId);

    /// <summary>Admin resets another user's password without needing current password.</summary>
    Task<Result> AdminResetPasswordAsync(Guid domainUserId, string newPassword);

    /// <summary>User changes their own password; requires current password verification.</summary>
    Task<Result> ChangeUserPasswordAsync(Guid domainUserId, string currentPassword, string newPassword);

    /// <summary>Updates a user's roles in Identity.</summary>
    Task<Result> UpdateUserRolesAsync(Guid domainUserId, IEnumerable<string> roles);

    /// <summary>Creates a role in Identity.</summary>
    Task<Result> CreateRoleAsync(string roleName);
}
