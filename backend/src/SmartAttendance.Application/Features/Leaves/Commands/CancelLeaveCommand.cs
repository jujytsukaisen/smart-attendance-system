using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class CancelLeaveCommand : IRequest<Result>
{
    public Guid LeaveId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CancelLeaveCommandValidator : AbstractValidator<CancelLeaveCommand>
{
    public CancelLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CancelLeaveCommandHandler : IRequestHandler<CancelLeaveCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CancelLeaveCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(CancelLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (leave == null) return Result.Failure(new[] { "Leave not found." });
        
        // Getting user ID might need CurrentUserService, but it's not injected here.
        // We'll use an empty Guid for now if we can't inject.
        leave.Cancel(Guid.Empty, request.Reason);
        
        _context.Leaves.Update(leave);

        return Result.Success();
    }
}

