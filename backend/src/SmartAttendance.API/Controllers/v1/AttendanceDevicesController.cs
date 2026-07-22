using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.AttendanceDevices.Commands;
using SmartAttendance.Application.Features.AttendanceDevices.DTOs;
using SmartAttendance.Application.Features.AttendanceDevices.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AttendanceDevicesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<AttendanceDeviceListDto>>>> GetAttendanceDevices([FromQuery] GetAttendanceDevicesQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<AttendanceDeviceDetailsDto>>> GetAttendanceDevice(Guid id)
    {
        return Ok(await Mediator.Send(new GetAttendanceDeviceByIdQuery(id)));
    }

    [HttpGet("company/{companyId:guid}")]
    public async Task<ActionResult<Result<System.Collections.Generic.List<AttendanceDeviceDto>>>> GetByCompany(Guid companyId)
    {
        return Ok(await Mediator.Send(new GetAttendanceDevicesByCompanyQuery(companyId)));
    }

    [HttpGet("branch/{branchId:guid}")]
    public async Task<ActionResult<Result<System.Collections.Generic.List<AttendanceDeviceDto>>>> GetByBranch(Guid branchId)
    {
        return Ok(await Mediator.Send(new GetAttendanceDevicesByBranchQuery(branchId)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttendanceDevice([FromBody] CreateAttendanceDeviceCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAttendanceDevice(Guid id, [FromBody] UpdateAttendanceDeviceCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(Result<MediatR.Unit>.Failure(new[] { "Id mismatch" }));
        }
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteAttendanceDeviceCommand(id)));
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreAttendanceDeviceCommand(id)));
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateAttendanceDeviceCommand(id)));
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateAttendanceDeviceCommand(id)));
    }
}
