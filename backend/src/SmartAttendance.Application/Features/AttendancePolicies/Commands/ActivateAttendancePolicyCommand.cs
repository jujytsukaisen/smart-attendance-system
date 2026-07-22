using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class ActivateAttendancePolicyCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class ActivateAttendancePolicyCommandValidator : AbstractValidator<ActivateAttendancePolicyCommand>
{
    public ActivateAttendancePolicyCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class ActivateAttendancePolicyCommandHandler : IRequestHandler<ActivateAttendancePolicyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateAttendancePolicyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateAttendancePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        
        if (policy == null)
            return Result.Failure(new[] { "Attendance policy not found." });

        policy.Activate();

        return Result.Success();
    }
}

