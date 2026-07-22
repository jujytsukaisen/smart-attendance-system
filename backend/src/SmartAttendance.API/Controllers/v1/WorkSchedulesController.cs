using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.WorkSchedules.Commands;
using SmartAttendance.Application.Features.WorkSchedules.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class WorkSchedulesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:WorkSchedules.View")]
    public async Task<IActionResult> GetSchedules([FromQuery] GetWorkSchedulesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:WorkSchedules.View")]
    public async Task<IActionResult> GetScheduleById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetWorkScheduleByIdQuery { WorkScheduleId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:WorkSchedules.Create")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateWorkScheduleCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:WorkSchedules.Update")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateWorkScheduleCommand command)
    {
        if (id != command.WorkScheduleId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:WorkSchedules.Delete")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteWorkScheduleCommand { WorkScheduleId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:WorkSchedules.Update")]
    public async Task<IActionResult> ActivateSchedule(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateWorkScheduleCommand { WorkScheduleId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:WorkSchedules.Update")]
    public async Task<IActionResult> DeactivateSchedule(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateWorkScheduleCommand { WorkScheduleId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:WorkSchedules.Delete")]
    public async Task<IActionResult> RestoreSchedule(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreWorkScheduleCommand { WorkScheduleId = id }));
    }
}

