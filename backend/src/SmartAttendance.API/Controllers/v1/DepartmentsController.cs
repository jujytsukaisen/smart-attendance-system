using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartAttendance.Application.Features.Departments.Commands;
using SmartAttendance.Application.Features.Departments.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class DepartmentsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Departments.View")]
    [OutputCache(PolicyName = "StandardCache")]
    public async Task<IActionResult> GetDepartments([FromQuery] GetDepartmentsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Departments.View")]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetDepartmentByIdQuery { DepartmentId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Departments.Create")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Departments.Update")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentCommand command)
    {
        if (id != command.DepartmentId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Departments.Delete")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteDepartmentCommand { DepartmentId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Departments.Update")]
    public async Task<IActionResult> RestoreDepartment(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreDepartmentCommand { DepartmentId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:Departments.Update")]
    public async Task<IActionResult> ActivateDepartment(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateDepartmentCommand { DepartmentId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:Departments.Update")]
    public async Task<IActionResult> DeactivateDepartment(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateDepartmentCommand { DepartmentId = id }));
    }
}

