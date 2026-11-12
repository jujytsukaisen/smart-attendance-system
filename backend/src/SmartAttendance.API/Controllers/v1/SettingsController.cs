using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Settings.Commands;
using SmartAttendance.Application.Features.Settings.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class SettingsController : ApiControllerBase
{
    // ────────────────────────────────────────────────
    // System (aggregate all sections)
    // ────────────────────────────────────────────────

    [HttpGet("system")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetSystemSettings()
    {
        return HandleResult(await Mediator.Send(new GetSystemSettingsQuery()));
    }

    // ────────────────────────────────────────────────
    // General
    // ────────────────────────────────────────────────

    [HttpGet("general")]
    public async Task<IActionResult> GetGeneralSettings()
    {
        return HandleResult(await Mediator.Send(new GetGeneralSettingsQuery()));
    }

    [HttpPut("general")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateGeneralSettings([FromBody] UpdateGeneralSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Attendance
    // ────────────────────────────────────────────────

    [HttpGet("attendance")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetAttendanceSettings()
    {
        return HandleResult(await Mediator.Send(new GetAttendanceSettingsQuery()));
    }

    [HttpPut("attendance")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateAttendanceSettings([FromBody] UpdateAttendanceSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Security
    // ────────────────────────────────────────────────

    [HttpGet("security")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetSecuritySettings()
    {
        return HandleResult(await Mediator.Send(new GetSecuritySettingsQuery()));
    }

    [HttpPut("security")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateSecuritySettings([FromBody] UpdateSecuritySettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Notifications
    // ────────────────────────────────────────────────

    [HttpGet("notifications")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetNotificationSettings()
    {
        return HandleResult(await Mediator.Send(new GetNotificationSettingsQuery()));
    }

    [HttpPut("notifications")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Devices
    // ────────────────────────────────────────────────

    [HttpGet("devices")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetDeviceSettings()
    {
        return HandleResult(await Mediator.Send(new GetDeviceSettingsQuery()));
    }

    [HttpPut("devices")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateDeviceSettings([FromBody] UpdateDeviceSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Email
    // ────────────────────────────────────────────────

    [HttpGet("email")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetEmailSettings()
    {
        return HandleResult(await Mediator.Send(new GetEmailSettingsQuery()));
    }

    [HttpPut("email")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateEmailSettings([FromBody] UpdateEmailSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // SMS
    // ────────────────────────────────────────────────

    [HttpGet("sms")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetSmsSettings()
    {
        return HandleResult(await Mediator.Send(new GetSmsSettingsQuery()));
    }

    [HttpPut("sms")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateSmsSettings([FromBody] UpdateSmsSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Backup
    // ────────────────────────────────────────────────

    [HttpGet("backup")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetBackupSettings()
    {
        return HandleResult(await Mediator.Send(new GetBackupSettingsQuery()));
    }

    [HttpPut("backup")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateBackupSettings([FromBody] UpdateBackupSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Face Recognition
    // ────────────────────────────────────────────────

    [HttpGet("face-recognition")]
    [Authorize(Policy = "Permission:Settings.View")]
    public async Task<IActionResult> GetFaceRecognitionSettings()
    {
        return HandleResult(await Mediator.Send(new GetFaceRecognitionSettingsQuery()));
    }

    [HttpPut("face-recognition")]
    [Authorize(Policy = "Permission:Settings.Update")]
    public async Task<IActionResult> UpdateFaceRecognitionSettings([FromBody] UpdateFaceRecognitionSettingsCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    // ────────────────────────────────────────────────
    // Reset (delete DB rows → next GET returns defaults)
    // ────────────────────────────────────────────────

    /// <summary>
    /// Resets a single settings section for the current tenant.
    /// The <paramref name="section"/> value must be one of the camelCase
    /// section keys: general, attendance, security, notifications,
    /// devices, email, sms, backup, faceRecognition.
    /// </summary>
    [HttpPost("{section}/reset")]
    [Authorize(Policy = "Permission:Settings.Manage")]
    public async Task<IActionResult> ResetSection(string section)
    {
        return HandleResult(await Mediator.Send(new ResetSectionSettingsCommand { Section = section }));
    }

    /// <summary>
    /// Resets ALL settings sections for the current tenant.
    /// </summary>
    [HttpPost("reset-all")]
    [Authorize(Policy = "Permission:Settings.Manage")]
    public async Task<IActionResult> ResetAll()
    {
        return HandleResult(await Mediator.Send(new ResetAllSettingsCommand()));
    }
}
