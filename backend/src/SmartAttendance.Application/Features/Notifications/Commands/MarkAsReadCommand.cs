using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class MarkAsReadCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

public class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);
        if (notification == null) return Result.Failure(new[] { "Notification not found." });
        
        notification.MarkAsRead();
        _context.Notifications.Update(notification);

        return Result.Success();
    }
}

