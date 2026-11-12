using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Infrastructure.Services.Email;
using SmartAttendance.Infrastructure.Services.Notification.Providers;
using SmartAttendance.Infrastructure.Services.SMS;
using Microsoft.EntityFrameworkCore;

namespace SmartAttendance.Infrastructure.Services.Notifications;

public class NotificationDispatcher : INotificationService
{
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly INotificationProvider _provider;
    private readonly IApplicationDbContext _dbContext;

    public NotificationDispatcher(
        ILogger<NotificationDispatcher> logger,
        IEmailService emailService,
        ISmsService smsService,
        INotificationProvider provider,
        IApplicationDbContext dbContext)
    {
        _logger = logger;
        _emailService = emailService;
        _smsService = smsService;
        _provider = provider;
        _dbContext = dbContext;
    }

    public async Task<Result> SendNotificationAsync(string userId, string title, string message)
    {
        _logger.LogInformation("Dispatching notification to user {UserId}: {Title} - {Message}", userId, title, message);

        if (Guid.TryParse(userId, out var userGuid))
        {
            string? emailAddress = null;

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
            if (user != null && user.Email != null && !string.IsNullOrWhiteSpace(user.Email.Value))
            {
                emailAddress = user.Email.Value;
            }
            else
            {
                var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == userGuid);
                if (employee != null && employee.Email != null && !string.IsNullOrWhiteSpace(employee.Email.Value))
                {
                    emailAddress = employee.Email.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                try
                {
                    await _emailService.SendAsync(emailAddress, title, message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email notification to {Email}", emailAddress);
                }
            }
        }

        return await _provider.SendNotificationAsync(userId, title, message);
    }
}
