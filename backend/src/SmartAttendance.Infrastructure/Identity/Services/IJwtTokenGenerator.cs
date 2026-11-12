using System;
using System.Collections.Generic;
using System.Security.Claims;
using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Identity.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions, Guid? companyId = null);
    RefreshToken GenerateRefreshToken(Guid userId);
}
