using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Commands;

public class DeleteReadNotificationsCommand : IRequest<Result>
{
}

public class DeleteReadNotificationsCommandHandler : IRequestHandler<DeleteReadNotificationsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteReadNotificationsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteReadNotificationsCommand request, CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);

        var readNotifications = await _context.Notifications
            .Where(x => x.RecipientUserId == currentUserId && x.Status == SmartAttendance.Domain.Enums.NotificationStatus.Read)
            .ToListAsync(cancellationToken);

        if (readNotifications.Any())
        {
            _context.Notifications.RemoveRange(readNotifications);
        }

        return Result.Success();
    }
}
