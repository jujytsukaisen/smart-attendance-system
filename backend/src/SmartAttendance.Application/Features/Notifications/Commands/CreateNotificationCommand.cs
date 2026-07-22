using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class CreateNotificationCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
}

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RecipientUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.NotificationType).NotEmpty();
    }
}

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notificationType = Enum.TryParse<SmartAttendance.Domain.Enums.NotificationType>(request.NotificationType, out var type) 
            ? type 
            : SmartAttendance.Domain.Enums.NotificationType.System;
            
        var notification = SmartAttendance.Domain.Entities.Notification.Create(
            request.CompanyId,
            request.RecipientUserId,
            request.Title,
            request.Message,
            notificationType,
            SmartAttendance.Domain.Enums.NotificationPriority.Medium);
            
        await _context.Notifications.AddAsync(notification, cancellationToken);

        return Result<Guid>.Success(notification.Id);
    }
}

