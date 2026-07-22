using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class DeactivateAttendancePolicyCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class DeactivateAttendancePolicyCommandValidator : AbstractValidator<DeactivateAttendancePolicyCommand>
{
    public DeactivateAttendancePolicyCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class DeactivateAttendancePolicyCommandHandler : IRequestHandler<DeactivateAttendancePolicyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateAttendancePolicyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateAttendancePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        
        if (policy == null)
            return Result.Failure(new[] { "Attendance policy not found." });

        policy.Deactivate();

        return Result.Success();
    }
}

