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
    /// <summary>
    /// Returns only the current logged-in employee's own leave records (self-service).
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyLeaves([FromQuery] GetMyLeavesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

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

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Leaves.Update")]
    public async Task<IActionResult> UpdateLeave(Guid id, [FromBody] UpdateLeaveCommand command)
    {
        command.LeaveId = id;
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = "Permission:Leaves.Approve")]
    public async Task<IActionResult> ApproveLeave(Guid id, [FromBody] ApproveLeaveRequest? request = null)
    {
        return HandleResult(await Mediator.Send(new ApproveLeaveCommand 
        { 
            LeaveId = id, 
            Comment = request?.Comment ?? string.Empty 
        }));
    }

    [HttpPost("{id}/reject")]
    [Authorize(Policy = "Permission:Leaves.Reject")]
    public async Task<IActionResult> RejectLeave(Guid id, [FromBody] RejectLeaveRequest? request = null)
    {
        return HandleResult(await Mediator.Send(new RejectLeaveCommand 
        { 
            LeaveId = id, 
            Reason = !string.IsNullOrWhiteSpace(request?.Reason) ? request.Reason : "Rejected by manager" 
        }));
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Policy = "Permission:Leaves.Update")]
    public async Task<IActionResult> CancelLeave(Guid id, [FromBody] CancelLeaveRequest? request = null)
    {
        return HandleResult(await Mediator.Send(new CancelLeaveCommand 
        { 
            LeaveId = id, 
            Reason = !string.IsNullOrWhiteSpace(request?.Reason) ? request.Reason : "Cancelled by user" 
        }));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Leaves.Delete")]
    public async Task<IActionResult> DeleteLeave(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteLeaveCommand { LeaveId = id }));
    }
}

public class ApproveLeaveRequest
{
    public string? Comment { get; set; }
}

public class RejectLeaveRequest
{
    public string? Reason { get; set; }
}

public class CancelLeaveRequest
{
    public string? Reason { get; set; }
}

