using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
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

    // ⚠️ تغيّر من [FromForm] (صورة) إلى JSON body (بصمة 128 رقم من face-api.js)
    [HttpPost]
    [Authorize(Policy = "Permission:FaceProfiles.Create")]
    public async Task<IActionResult> RegisterFace([FromBody] RegisterFaceCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ✅ جديد: نقطة تحقق تُستخدم عند تسجيل الحضور بالوجه
    [HttpPost("verify")]
    [Authorize(Policy = "Permission:FaceProfiles.View")]
    public async Task<IActionResult> VerifyFace([FromBody] VerifyFaceCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ✅ Kiosk Mode: جلب جميع بصمات الوجه للمقارنة المحلية بدون معرفة هوية الموظف مسبقاً
    [HttpGet("encodings/all")]
    [Authorize(Policy = "Permission:FaceProfiles.View")]
    public async Task<IActionResult> GetAllFaceEncodings()
    {
        return HandleResult(await Mediator.Send(new GetAllFaceEncodingsQuery()));
    }

    [HttpDelete("{employeeId}")]
    [Authorize(Policy = "Permission:FaceProfiles.Delete")]
    public async Task<IActionResult> DeleteFaceProfile(Guid employeeId)
    {
        return HandleResult(await Mediator.Send(new DeleteFaceProfileCommand { FaceProfileId = employeeId }));
    }
}