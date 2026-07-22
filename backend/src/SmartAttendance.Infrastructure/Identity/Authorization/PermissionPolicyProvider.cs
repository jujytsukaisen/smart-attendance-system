using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SmartAttendance.Infrastructure.Identity.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Permission", StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();
            // Expected format Permission:Attendance.CheckIn
            var permission = policyName.Substring("Permission:".Length);
            policy.AddRequirements(new PermissionRequirement(permission));
            return policy.Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}
