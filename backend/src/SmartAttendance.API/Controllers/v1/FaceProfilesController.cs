using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.FaceProfiles.Commands;
using SmartAttendance.Application.Features.FaceProfiles.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class FaceProfilesController : ApiControllerBase
{
    [HttpGet("{employeeId}")]
    [Authorize(Policy = "Permission:FaceProfiles.View")]
    public async Task<IActionResult> GetFaceProfile(Guid employeeId)
    {
        return HandleResult(await Mediator.Send(new GetFaceProfileByEmployeeQuery { EmployeeId = employeeId }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:FaceProfiles.Create")]
    public async Task<IActionResult> RegisterFace([FromForm] RegisterFaceCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{employeeId}")]
    [Authorize(Policy = "Permission:FaceProfiles.Delete")]
    public async Task<IActionResult> DeleteFaceProfile(Guid employeeId)
    {
        return HandleResult(await Mediator.Send(new DeleteFaceProfileCommand { FaceProfileId = employeeId }));
    }
}

