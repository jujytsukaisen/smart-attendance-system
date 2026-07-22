using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Breaks.Commands;
using SmartAttendance.Application.Features.Breaks.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class BreaksController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Breaks.View")]
    public async Task<IActionResult> GetBreaks([FromQuery] GetBreaksQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Breaks.View")]
    public async Task<IActionResult> GetBreakById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetBreakByIdQuery { BreakId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Breaks.Create")]
    public async Task<IActionResult> CreateBreak([FromBody] CreateBreakCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Breaks.Update")]
    public async Task<IActionResult> UpdateBreak(Guid id, [FromBody] UpdateBreakCommand command)
    {
        if (id != command.BreakId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Breaks.Delete")]
    public async Task<IActionResult> DeleteBreak(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteBreakCommand { BreakId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Breaks.Update")]
    public async Task<IActionResult> RestoreBreak(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreBreakCommand { BreakId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:Breaks.Update")]
    public async Task<IActionResult> ActivateBreak(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateBreakCommand { BreakId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:Breaks.Update")]
    public async Task<IActionResult> DeactivateBreak(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateBreakCommand { BreakId = id }));
    }

    [HttpGet("shift/{shiftId}")]
    [Authorize(Policy = "Permission:Breaks.View")]
    public async Task<IActionResult> GetBreaksByShift(Guid shiftId, [FromQuery] GetBreaksByShiftQuery query)
    {
        if (shiftId != query.ShiftId)
        {
            query.ShiftId = shiftId;
        }
        return HandleResult(await Mediator.Send(query));
    }
}

