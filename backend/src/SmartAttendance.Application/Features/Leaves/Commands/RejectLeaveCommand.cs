using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class RejectLeaveCommand : IRequest<Result>
{
    public Guid LeaveId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RejectLeaveCommandValidator : AbstractValidator<RejectLeaveCommand>
{
    public RejectLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class RejectLeaveCommandHandler : IRequestHandler<RejectLeaveCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public RejectLeaveCommandHandler(IApplicationDbContext context, INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _context = context;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (leave == null) return Result.Failure(new[] { "Leave not found." });
        
        var userIdStr = _currentUserService.UserId;
        Guid.TryParse(userIdStr, out var userId);
        
        leave.Reject(userId, request.Reason);
        
        _context.Leaves.Update(leave);

        var notificationResult = await _notificationService.SendNotificationAsync(leave.EmployeeId.ToString(), "Leave Rejected", "Your leave has been rejected.");
        if (!notificationResult.Succeeded)
        {
            return notificationResult;
        }
        
        return Result.Success();
    }
}

