using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.AuditLogs.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class AuditLogsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:AuditLogs.View")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:AuditLogs.View")]
    public async Task<IActionResult> GetAuditLogById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetAuditLogByIdQuery { AuditLogId = id }));
    }
}

