using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class DisableFaceRecognitionCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
}

public class DisableFaceRecognitionCommandValidator : AbstractValidator<DisableFaceRecognitionCommand>
{
    public DisableFaceRecognitionCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class DisableFaceRecognitionCommandHandler : IRequestHandler<DisableFaceRecognitionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DisableFaceRecognitionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DisableFaceRecognitionCommand request, CancellationToken cancellationToken)
    {
        var policy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        if (policy == null) return Result.Failure(new[] { "Attendance policy not found." });

        policy.DisableFaceRecognition();

        return Result.Success();
    }
}

