using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Permissions.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class PermissionsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Permissions.View")]
    public async Task<IActionResult> GetPermissions([FromQuery] GetPermissionsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Permissions.View")]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetPermissionByIdQuery { PermissionId = id }));
    }
}

