using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Shifts.Commands;
using SmartAttendance.Application.Features.Shifts.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class ShiftsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Shifts.View")]
    public async Task<IActionResult> GetShifts([FromQuery] GetShiftsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Shifts.View")]
    public async Task<IActionResult> GetShiftById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetShiftByIdQuery { ShiftId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Shifts.Create")]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Shifts.Update")]
    public async Task<IActionResult> UpdateShift(Guid id, [FromBody] UpdateShiftCommand command)
    {
        if (id != command.ShiftId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Shifts.Delete")]
    public async Task<IActionResult> DeleteShift(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteShiftCommand { ShiftId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:Shifts.Update")]
    public async Task<IActionResult> ActivateShift(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateShiftCommand { ShiftId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:Shifts.Update")]
    public async Task<IActionResult> DeactivateShift(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateShiftCommand { ShiftId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Shifts.Update")]
    public async Task<IActionResult> RestoreShift(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreShiftCommand { ShiftId = id }));
    }

    [HttpGet("work-schedule/{workScheduleId}")]
    [Authorize(Policy = "Permission:Shifts.View")]
    public async Task<IActionResult> GetShiftsByWorkSchedule(Guid workScheduleId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return HandleResult(await Mediator.Send(new GetShiftsByWorkScheduleQuery { WorkScheduleId = workScheduleId, PageNumber = pageNumber, PageSize = pageSize }));
    }
}

