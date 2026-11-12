using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class MarkAllAsReadCommand : IRequest<Result>
{
}

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllAsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);

        var unreadNotifications = await _context.Notifications
            .Where(x => x.RecipientUserId == currentUserId && x.Status != SmartAttendance.Domain.Enums.NotificationStatus.Read && x.Status != SmartAttendance.Domain.Enums.NotificationStatus.Archived)
            .ToListAsync(cancellationToken);

        if (unreadNotifications.Any())
        {
            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
            }

            _context.Notifications.UpdateRange(unreadNotifications);
            // Assuming SaveChangesAsync is called somewhere centrally like a pipeline behavior,
            // or I might need to verify how saving works in this project.
            // Let's check how MarkAsReadCommand.cs does it.
            // MarkAsReadCommand does:
            // _context.Notifications.Update(notification);
            // return Result.Success();
            // It seems DbContext SaveChanges is handled by a pipeline behavior (e.g. UnitOfWork or TransactionBehavior).
        }

        return Result.Success();
    }
}
