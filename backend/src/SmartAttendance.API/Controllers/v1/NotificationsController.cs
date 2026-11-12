using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Notifications.Commands;
using SmartAttendance.Application.Features.Notifications.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var query = new GetNotificationsQuery
        {
            Status = "Unread",
            PageNumber = 1,
            PageSize = 1
        };

        var result = await Mediator.Send(query);

        if (!result.Succeeded)
            return HandleResult(result);

        return HandleResult(Result<int>.Success(result.Data!.UnreadCount));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotificationById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetNotificationByIdQuery { NotificationId = id }));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        return HandleResult(await Mediator.Send(new MarkAsReadCommand { NotificationId = id }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteNotificationCommand { NotificationId = id }));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        return HandleResult(await Mediator.Send(new MarkAllAsReadCommand()));
    }

    [HttpDelete("read")]
    public async Task<IActionResult> DeleteReadNotifications()
    {
        return HandleResult(await Mediator.Send(new DeleteReadNotificationsCommand()));
    }
}