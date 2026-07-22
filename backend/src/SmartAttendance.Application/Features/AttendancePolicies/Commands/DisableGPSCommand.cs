using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class DisableGPSCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class DisableGPSCommandValidator : AbstractValidator<DisableGPSCommand>
{
    public DisableGPSCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class DisableGPSCommandHandler : IRequestHandler<DisableGPSCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DisableGPSCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DisableGPSCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        if (policy == null) return Result.Failure(new[] { "Attendance policy not found." });

        policy.DisableGPS();

        return Result.Success();
    }
}

