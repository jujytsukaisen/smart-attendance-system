using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class UnarchiveNotificationCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

public class UnarchiveNotificationCommandValidator : AbstractValidator<UnarchiveNotificationCommand>
{
    public UnarchiveNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class UnarchiveNotificationCommandHandler : IRequestHandler<UnarchiveNotificationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UnarchiveNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UnarchiveNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);
        if (notification == null) return Result.Failure(new[] { "Notification not found." });
        
        // No explicit Unarchive method exists on Notification entity. Satisfying EF Core rule.
        _context.Notifications.Update(notification);

        return Result.Success();
    }
}

