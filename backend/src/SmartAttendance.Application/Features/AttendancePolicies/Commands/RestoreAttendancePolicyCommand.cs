using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class RestoreAttendancePolicyCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class RestoreAttendancePolicyCommandValidator : AbstractValidator<RestoreAttendancePolicyCommand>
{
    public RestoreAttendancePolicyCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class RestoreAttendancePolicyCommandHandler : IRequestHandler<RestoreAttendancePolicyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreAttendancePolicyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreAttendancePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.AttendancePolicyId, cancellationToken);
        
        if (policy == null)
            return Result.Failure(new[] { "Attendance policy not found." });

        policy.Restore();

        return Result.Success();
    }
}
