using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartAttendance.Application.Features.Branches.Commands;
using SmartAttendance.Application.Features.Branches.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class BranchesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Branches.View")]
    [OutputCache(PolicyName = "StandardCache")]
    public async Task<IActionResult> GetBranches([FromQuery] GetBranchesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Branches.View")]
    public async Task<IActionResult> GetBranchById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetBranchByIdQuery { BranchId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Branches.Create")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Branches.Update")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] UpdateBranchCommand command)
    {
        if (id != command.BranchId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Branches.Delete")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteBranchCommand { BranchId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Branches.Restore")]
    public async Task<IActionResult> RestoreBranch(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreBranchCommand { BranchId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:Branches.Update")]
    public async Task<IActionResult> ActivateBranch(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateBranchCommand { BranchId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:Branches.Update")]
    public async Task<IActionResult> DeactivateBranch(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateBranchCommand { BranchId = id }));
    }
}

