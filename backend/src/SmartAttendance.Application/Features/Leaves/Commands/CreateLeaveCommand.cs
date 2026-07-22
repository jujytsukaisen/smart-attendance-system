using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
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
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
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

    public CreateLeaveCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = SmartAttendance.Domain.Entities.Leave.Request(
            request.CompanyId,
            request.EmployeeId,
            Enum.Parse<SmartAttendance.Domain.Enums.LeaveType>(request.LeaveType),
            request.StartDate,
            request.EndDate,
            request.Reason);

        await _context.Leaves.AddAsync(leave, cancellationToken);

        // Simulate a notification call
        var notificationResult = await _notificationService.SendNotificationAsync(request.EmployeeId.ToString(), "Leave Requested", "Your leave request has been submitted.");
        if (!notificationResult.Succeeded)
        {
            return Result<Guid>.Failure(notificationResult.Errors);
        }

        return Result<Guid>.Success(leave.Id);
    }
}

