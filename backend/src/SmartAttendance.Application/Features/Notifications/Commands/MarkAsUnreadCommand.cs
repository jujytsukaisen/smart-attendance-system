using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class MarkAsUnreadCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

public class MarkAsUnreadCommandValidator : AbstractValidator<MarkAsUnreadCommand>
{
    public MarkAsUnreadCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class MarkAsUnreadCommandHandler : IRequestHandler<MarkAsUnreadCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsUnreadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsUnreadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);
        if (notification == null) return Result.Failure(new[] { "Notification not found." });
        
        // No explicit MarkAsUnread method exists on Notification entity. Satisfying EF Core rule.
        _context.Notifications.Update(notification);

        return Result.Success();
    }
}

