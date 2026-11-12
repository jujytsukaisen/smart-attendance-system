using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class CreateLeaveCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateLeaveCommandValidator : AbstractValidator<CreateLeaveCommand>
{
    public CreateLeaveCommandValidator()
    {
        RuleFor(x => x.EmployeeId); // Optional for employees, handled in handler
        RuleFor(x => x.LeaveType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CreateLeaveCommandHandler : IRequestHandler<CreateLeaveCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public CreateLeaveCommandHandler(
        IApplicationDbContext context, 
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveCommand request, CancellationToken cancellationToken)
    {
        // المستخدم الحالي (Guid) من التوكن
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);

        if (request.EmployeeId == Guid.Empty)
        {
            var currentUserEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == currentUserId, cancellationToken);
            if (currentUserEmployee == null)
            {
                return Result<Guid>.Failure(new[] { "User is not associated with any employee record." });
            }
            request.EmployeeId = currentUserEmployee.Id;
        }

        var companyId = request.CompanyId;
        if (companyId == Guid.Empty && Guid.TryParse(_currentUserService.TenantId, out var tenantGuid))
        {
            companyId = tenantGuid;
        }

        if (companyId == Guid.Empty)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId || e.UserId == request.EmployeeId, cancellationToken);
            if (employee != null)
            {
                companyId = employee.CompanyId;
                // Update the request to use the actual EmployeeId in case UserId was passed
                request.EmployeeId = employee.Id;
            }
        }
        else
        {
            // Even if we have companyId, if the user sent their UserId, we should map it to EmployeeId
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId || e.UserId == request.EmployeeId, cancellationToken);
            if (employee != null)
            {
                request.EmployeeId = employee.Id;
            }
        }

        SmartAttendance.Domain.Enums.LeaveType leaveType;
        if (int.TryParse(request.LeaveType, out var enumInt) && Enum.IsDefined(typeof(SmartAttendance.Domain.Enums.LeaveType), enumInt))
        {
            leaveType = (SmartAttendance.Domain.Enums.LeaveType)enumInt;
        }
        else if (!Enum.TryParse<SmartAttendance.Domain.Enums.LeaveType>(request.LeaveType, true, out leaveType))
        {
            leaveType = SmartAttendance.Domain.Enums.LeaveType.Annual;
        }

        var leave = SmartAttendance.Domain.Entities.Leave.Request(
            companyId,
            request.EmployeeId,
            leaveType,
            request.StartDate,
            request.EndDate,
            request.Reason);

        await _context.Leaves.AddAsync(leave, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var notifEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        
        // اجمع معرفات المستخدمين الذين يجب أن يتلقوا الإشعار
        var recipientIds = new System.Collections.Generic.HashSet<Guid>();
        
        // أضف المستخدم الحالي دائماً (Admin/HR الذي أنشأ الطلب)
        if (currentUserId != Guid.Empty)
            recipientIds.Add(currentUserId);
        
        // أضف الموظف إذا كان مرتبطاً بمستخدم وكان مختلفاً عن المستخدم الحالي
        if (notifEmployee?.UserId.HasValue == true && notifEmployee.UserId.Value != currentUserId)
            recipientIds.Add(notifEmployee.UserId.Value);

        foreach (var recipientId in recipientIds)
        {
            var notification = SmartAttendance.Domain.Entities.Notification.Create(
                companyId,
                recipientId,
                "Leave Requested",
                $"Leave request submitted for {notifEmployee?.FirstName} {notifEmployee?.LastName} ({leave.LeaveType}): {leave.StartDate:MMM dd} - {leave.EndDate:MMM dd}.",
                SmartAttendance.Domain.Enums.NotificationType.System,
                SmartAttendance.Domain.Enums.NotificationPriority.Medium);

            await _context.Notifications.AddAsync(notification, cancellationToken);
        }
        
        if (recipientIds.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _notificationService.SendNotificationAsync(request.EmployeeId.ToString(), "Leave Requested", "Your leave request has been submitted.");
        }
        catch
        {
            // Ignore notification failure if any
        }
        return Result<Guid>.Success(leave.Id);
    }
}

