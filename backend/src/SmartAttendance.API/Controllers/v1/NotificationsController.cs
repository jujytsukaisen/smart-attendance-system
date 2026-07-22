using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Notifications.Commands;
using SmartAttendance.Application.Features.Notifications.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Notifications.View")]
    public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Notifications.View")]
    public async Task<IActionResult> GetNotificationById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetNotificationByIdQuery { NotificationId = id }));
    }

    [HttpPost("{id}/read")]
    [Authorize(Policy = "Permission:Notifications.Update")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        return HandleResult(await Mediator.Send(new MarkAsReadCommand { NotificationId = id }));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Notifications.Delete")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteNotificationCommand { NotificationId = id }));
    }
}

