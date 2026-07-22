using System;

namespace SmartAttendance.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? TenantId { get; }
    string? CurrentUserId { get; }
    string? UserName { get; }
    string? Email { get; }
    Guid? CompanyId { get; }
    Guid? BranchId { get; }
    Guid? DepartmentId { get; }
    System.Collections.Generic.IEnumerable<string> Roles { get; }
    System.Collections.Generic.IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
