using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Commands;

public class UpdateLeaveCommand : IRequest<Result>
{
    public Guid LeaveId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UpdateLeaveCommandValidator : AbstractValidator<UpdateLeaveCommand>
{
    public UpdateLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
        RuleFor(x => x.LeaveType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class UpdateLeaveCommandHandler : IRequestHandler<UpdateLeaveCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateLeaveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
        if (leave == null) return Result.Failure(new[] { "Leave not found." });
        
        leave.UpdateReason(request.Reason);
        
        _context.Leaves.Update(leave);

        return Result.Success();
    }
}



