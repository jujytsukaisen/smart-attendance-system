using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class ApproveLeaveCommand : IRequest<Result>
{
    public Guid LeaveId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ApproveLeaveCommandValidator : AbstractValidator<ApproveLeaveCommand>
{
    public ApproveLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}

public class ApproveLeaveCommandHandler : IRequestHandler<ApproveLeaveCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public ApproveLeaveCommandHandler(IApplicationDbContext context, INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _context = context;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (leave == null) return Result.Failure(new[] { "Leave not found." });
        
        var userIdStr = _currentUserService.UserId;
        Guid.TryParse(userIdStr, out var userId);
        
        leave.Approve(userId, request.Comment);
        
        _context.Leaves.Update(leave);

        var notificationResult = await _notificationService.SendNotificationAsync(leave.EmployeeId.ToString(), "Leave Approved", "Your leave has been approved.");
        if (!notificationResult.Succeeded)
        {
            // Proceed with success even if notification fails, but you might want to log it in a real app.
            // Or return failure depending on business rules. We will return failure if notification fails as requested "gracefully return their Results without breaking".
            // Since it's an audit fix, we just return the result. Actually, better to just log and continue or return the failure? "gracefully return their Results without breaking"
            // Wait, returning the failure might break the flow if it's considered non-critical. But the instructions say "All commands using them must gracefully return their Results without breaking."
            // I'll just return the notification result if it failed.
            return notificationResult;
        }
        
        return Result.Success();
    }
}

