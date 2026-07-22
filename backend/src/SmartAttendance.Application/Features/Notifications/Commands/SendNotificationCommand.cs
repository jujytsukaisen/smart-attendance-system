using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class SendNotificationCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public SendNotificationCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);
        if (notification == null) return Result.Failure(new[] { "Notification not found." });
        
        notification.MarkAsSent();
        _context.Notifications.Update(notification);

        var notificationResult = await _notificationService.SendNotificationAsync(notification.RecipientUserId.ToString(), notification.Title, notification.Message);
        if (!notificationResult.Succeeded)
        {
            return notificationResult;
        }
        
        return Result.Success();
    }
}

