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
    [Authorize(Policy = "Permission:Attendance.Restore")]
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
}
