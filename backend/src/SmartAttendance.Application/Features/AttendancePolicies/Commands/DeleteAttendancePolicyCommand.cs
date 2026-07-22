using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class DeleteAttendancePolicyCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class DeleteAttendancePolicyCommandValidator : AbstractValidator<DeleteAttendancePolicyCommand>
{
    public DeleteAttendancePolicyCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class DeleteAttendancePolicyCommandHandler : IRequestHandler<DeleteAttendancePolicyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteAttendancePolicyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteAttendancePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        
        if (policy == null)
            return Result.Failure(new[] { "Attendance policy not found." });

        policy.Delete();

        return Result.Success();
    }
}



