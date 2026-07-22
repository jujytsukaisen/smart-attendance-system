using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.AttendancePolicies.Commands;
using SmartAttendance.Application.Features.AttendancePolicies.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class AttendancePoliciesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:AttendancePolicies.View")]
    public async Task<IActionResult> GetPolicies([FromQuery] GetAttendancePoliciesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }



    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:AttendancePolicies.View")]
    public async Task<IActionResult> GetPolicyById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetAttendancePolicyByIdQuery { AttendancePolicyId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:AttendancePolicies.Create")]
    public async Task<IActionResult> CreatePolicy([FromBody] CreateAttendancePolicyCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:AttendancePolicies.Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendancePolicyCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:AttendancePolicies.Update")]
    public async Task<IActionResult> Activate(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateAttendancePolicyCommand { AttendancePolicyId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:AttendancePolicies.Update")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateAttendancePolicyCommand { AttendancePolicyId = id }));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:AttendancePolicies.Delete")]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteAttendancePolicyCommand { AttendancePolicyId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:AttendancePolicies.Update")]
    public async Task<IActionResult> RestorePolicy(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreAttendancePolicyCommand { AttendancePolicyId = id }));
    }
}

