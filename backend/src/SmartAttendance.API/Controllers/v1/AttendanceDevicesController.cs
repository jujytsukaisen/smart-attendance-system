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
    [Authorize(Policy = "Permission:AttendanceDevices.View")]
    public async Task<ActionResult<Result<PagedResult<AttendanceDeviceListDto>>>> GetAttendanceDevices([FromQuery] GetAttendanceDevicesQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:AttendanceDevices.View")]
    public async Task<ActionResult<Result<AttendanceDeviceDetailsDto>>> GetAttendanceDevice(Guid id)
    {
        return Ok(await Mediator.Send(new GetAttendanceDeviceByIdQuery(id)));
    }

    [HttpGet("company/{companyId:guid}")]
    [Authorize(Policy = "Permission:AttendanceDevices.View")]
    public async Task<ActionResult<Result<System.Collections.Generic.List<AttendanceDeviceDto>>>> GetByCompany(Guid companyId)
    {
        return Ok(await Mediator.Send(new GetAttendanceDevicesByCompanyQuery(companyId)));
    }

    [HttpGet("branch/{branchId:guid}")]
    [Authorize(Policy = "Permission:AttendanceDevices.View")]
    public async Task<ActionResult<Result<System.Collections.Generic.List<AttendanceDeviceDto>>>> GetByBranch(Guid branchId)
    {
        return Ok(await Mediator.Send(new GetAttendanceDevicesByBranchQuery(branchId)));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:AttendanceDevices.Create")]
    public async Task<IActionResult> CreateAttendanceDevice([FromBody] CreateAttendanceDeviceCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:AttendanceDevices.Update")]
    public async Task<IActionResult> UpdateAttendanceDevice(Guid id, [FromBody] UpdateAttendanceDeviceCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(Result<MediatR.Unit>.Failure(new[] { "Id mismatch" }));
        }
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:AttendanceDevices.Delete")]
    public async Task<IActionResult> DeleteAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteAttendanceDeviceCommand(id)));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:AttendanceDevices.Update")]
    public async Task<IActionResult> RestoreAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreAttendanceDeviceCommand(id)));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:AttendanceDevices.Update")]
    public async Task<IActionResult> ActivateAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateAttendanceDeviceCommand(id)));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:AttendanceDevices.Update")]
    public async Task<IActionResult> DeactivateAttendanceDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateAttendanceDeviceCommand(id)));
    }
}
