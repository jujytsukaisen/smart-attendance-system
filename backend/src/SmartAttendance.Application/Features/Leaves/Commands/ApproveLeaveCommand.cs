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
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == leave.EmployeeId, cancellationToken);
        
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);
        var recipientIds = new System.Collections.Generic.HashSet<Guid>();
        
        // المعتمِد (الذي ضغط Approve)
        if (currentUserId != Guid.Empty)
            recipientIds.Add(currentUserId);
        
        // الموظف صاحب الإجازة إذا كان مرتبطاً بمستخدم
        if (employee?.UserId.HasValue == true && employee.UserId.Value != currentUserId)
            recipientIds.Add(employee.UserId.Value);

        foreach (var recipientId in recipientIds)
        {
            var notification = SmartAttendance.Domain.Entities.Notification.Create(
                leave.CompanyId,
                recipientId,
                "Leave Approved",
                $"Leave approved for {employee?.FirstName} {employee?.LastName} ({leave.LeaveType}): {leave.StartDate:MMM dd} - {leave.EndDate:MMM dd}.",
                SmartAttendance.Domain.Enums.NotificationType.System,
                SmartAttendance.Domain.Enums.NotificationPriority.Medium);

            await _context.Notifications.AddAsync(notification, cancellationToken);
        }
        
        if (recipientIds.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _notificationService.SendNotificationAsync(leave.EmployeeId.ToString(), "Leave Approved", "Your leave has been approved.");
        }
        catch
        {
            // Ignore notification failure
        }
        
        return Result.Success();
    }
}

