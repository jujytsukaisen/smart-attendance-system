using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmartAttendance.API.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var isSystemEndpoint = endpoint?.Metadata?.GetMetadata<SmartAttendance.API.Attributes.SystemEndpointAttribute>() != null || 
                               endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var companyId = context.User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyId) && !isSystemEndpoint)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await _next(context);
    }
}
