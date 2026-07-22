using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartAttendance.Application.Common.Interfaces;

namespace SmartAttendance.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("CompanyId");

    // Additional properties requested by Phase 3.2
    public string? CurrentUserId => UserId;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    public Guid? CompanyId => Guid.TryParse(TenantId, out var id) ? id : null;
    public Guid? BranchId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("BranchId"), out var id) ? id : null;
    public Guid? DepartmentId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("DepartmentId"), out var id) ? id : null;
    
    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value) ?? Enumerable.Empty<string>();

    public IEnumerable<string> Permissions => _httpContextAccessor.HttpContext?.User?.Claims
        .Where(c => c.Type == "Permission")
        .Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
