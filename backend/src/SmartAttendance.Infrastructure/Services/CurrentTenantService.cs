using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SmartAttendance.Infrastructure.Services;

public interface ICurrentTenantService
{
    Guid? CompanyId { get; }
    Guid? BranchId { get; }
    Guid? UserId { get; }
    IEnumerable<string> Roles { get; }
}

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CompanyId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("CompanyId"), out var id) ? id : null;
    public Guid? BranchId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("BranchId"), out var id) ? id : null;
    public Guid? UserId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value) ?? Enumerable.Empty<string>();
}
