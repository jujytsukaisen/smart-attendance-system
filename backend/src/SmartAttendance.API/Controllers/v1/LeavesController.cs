using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Leaves.Commands;
using SmartAttendance.Application.Features.Leaves.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class LeavesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Leaves.View")]
    public async Task<IActionResult> GetLeaves([FromQuery] GetLeavesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Leaves.View")]
    public async Task<IActionResult> GetLeaveById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetLeaveByIdQuery { LeaveId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Leaves.Create")]
    public async Task<IActionResult> RequestLeave([FromBody] CreateLeaveCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = "Permission:Leaves.Approve")]
    public async Task<IActionResult> ApproveLeave(Guid id)
    {
        return HandleResult(await Mediator.Send(new ApproveLeaveCommand { LeaveId = id }));
    }

    [HttpPost("{id}/reject")]
    [Authorize(Policy = "Permission:Leaves.Approve")]
    public async Task<IActionResult> RejectLeave(Guid id)
    {
        return HandleResult(await Mediator.Send(new RejectLeaveCommand { LeaveId = id }));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Leaves.Delete")]
    public async Task<IActionResult> CancelLeave(Guid id)
    {
        return HandleResult(await Mediator.Send(new CancelLeaveCommand { LeaveId = id }));
    }
}

