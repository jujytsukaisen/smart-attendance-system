using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class EnableGPSCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class EnableGPSCommandValidator : AbstractValidator<EnableGPSCommand>
{
    public EnableGPSCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class EnableGPSCommandHandler : IRequestHandler<EnableGPSCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public EnableGPSCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(EnableGPSCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        if (policy == null) return Result.Failure(new[] { "Attendance policy not found." });

        policy.EnableGPS();

        return Result.Success();
    }
}

