using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Attendances.Commands;
using SmartAttendance.Application.Features.Attendances.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class AttendancesController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Policy = "Permission:Attendance.Create")]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Attendance.Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendanceCommand command)
    {
        if (id != command.AttendanceId)
            return BadRequest();

        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Attendance.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteAttendanceCommand { AttendanceId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Attendance.Update")]
    public async Task<IActionResult> Restore(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreAttendanceCommand { AttendanceId = id }));
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = "Permission:Attendance.Approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        return HandleResult(await Mediator.Send(new ApproveAttendanceCommand { AttendanceId = id }));
    }

    [HttpPost("{id}/reject")]
    [Authorize(Policy = "Permission:Attendance.Reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        return HandleResult(await Mediator.Send(new RejectAttendanceCommand { AttendanceId = id }));
    }

    /// <summary>
    /// Returns only the current logged-in employee's own attendance records (self-service).
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAttendances([FromQuery] GetMyAttendancesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Returns the live attendance state of the currently logged-in employee for today.
    /// Values: NotCheckedIn | Working | OnBreak | Finished.
    /// No Permission policy required — any authenticated user can check their own state.
    /// </summary>
    [HttpGet("my/state")]
    public async Task<IActionResult> GetMyState()
    {
        return HandleResult(await Mediator.Send(new GetMyEmployeeStateQuery()));
    }

    [HttpGet]
    [Authorize(Policy = "Permission:Attendance.View")]
    public async Task<IActionResult> GetAttendances([FromQuery] GetAttendancesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Attendance.View")]
    public async Task<IActionResult> GetAttendanceById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetAttendanceByIdQuery { AttendanceId = id }));
    }

    [HttpPost("check-in")]
    [Authorize(Policy = "Permission:Attendance.Create")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("check-out")]
    [Authorize(Policy = "Permission:Attendance.Create")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ── New Multi-Scan Endpoints ────────────────────────────────────────────────

    /// <summary>
    /// Smart scan endpoint — automatically determines the correct attendance action
    /// (CheckIn / BreakStart / BreakEnd / CheckOut) based on the employee's current state.
    /// Replaces the need to call /check-in and /check-out separately.
    /// </summary>
    [HttpPost("scan")]
    [Authorize(Policy = "Permission:Attendance.Create")]
    public async Task<IActionResult> RecordScan([FromBody] RecordScanCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Returns the ordered event timeline for a specific attendance record.
    /// Useful for rendering: 08:00 CheckIn → 10:15 BreakStart → 16:00 CheckOut.
    /// </summary>
    [HttpGet("{id}/events")]
    [Authorize(Policy = "Permission:Attendance.View")]
    public async Task<IActionResult> GetAttendanceEvents(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetAttendanceEventsQuery { AttendanceId = id }));
    }

    /// <summary>
    /// Returns the live attendance state of a specific employee for today.
    /// Values: NotCheckedIn | Working | OnBreak | Finished.
    /// </summary>
    [HttpGet("employee/{employeeId}/state")]
    [Authorize(Policy = "Permission:Attendance.View")]
    public async Task<IActionResult> GetEmployeeState(Guid employeeId)
    {
        return HandleResult(await Mediator.Send(new GetCurrentEmployeeStateQuery { EmployeeId = employeeId }));
    }
}

