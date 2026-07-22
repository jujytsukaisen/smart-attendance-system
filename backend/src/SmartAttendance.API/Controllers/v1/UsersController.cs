using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Users.Commands;
using SmartAttendance.Application.Features.Users.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Users.View")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Users.View")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetUserByIdQuery { UserId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Users.Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Users.Update")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.UserId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Users.Delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteUserCommand { UserId = id }));
    }
}

