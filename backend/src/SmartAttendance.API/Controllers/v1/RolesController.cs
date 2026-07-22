using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Roles.Commands;
using SmartAttendance.Application.Features.Roles.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class RolesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Roles.View")]
    public async Task<IActionResult> GetRoles([FromQuery] GetRolesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Roles.View")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetRoleByIdQuery { RoleId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Roles.Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Roles.Update")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
    {
        if (id != command.RoleId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Roles.Delete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteRoleCommand { RoleId = id }));
    }
}

