using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class DeleteNotificationCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

public class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);
        if (notification == null) return Result.Failure(new[] { "Notification not found." });
        
        _context.Notifications.Remove(notification);

        return Result.Success();
    }
}



