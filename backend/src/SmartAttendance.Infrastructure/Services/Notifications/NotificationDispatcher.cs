using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Infrastructure.Services.Email;
using SmartAttendance.Infrastructure.Services.Notification.Providers;
using SmartAttendance.Infrastructure.Services.SMS;

namespace SmartAttendance.Infrastructure.Services.Notifications;

public class NotificationDispatcher : INotificationService
{
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly INotificationProvider _provider;

    public NotificationDispatcher(
        ILogger<NotificationDispatcher> logger,
        IEmailService emailService,
        ISmsService smsService,
        INotificationProvider provider)
    {
        _logger = logger;
        _emailService = emailService;
        _smsService = smsService;
        _provider = provider;
    }

    public async Task<Result> SendNotificationAsync(string userId, string title, string message)
    {
        _logger.LogInformation("Dispatching notification to user {UserId}: {Title} - {Message}", userId, title, message);

        return await _provider.SendNotificationAsync(userId, title, message);
    }
}
