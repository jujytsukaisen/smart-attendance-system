using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Commands;

public class VerifyDeviceCommand : IRequest<Result>
{
    public Guid AttendanceId { get; set; }
    public Guid AttendanceDeviceId { get; set; }
}

public class VerifyDeviceCommandValidator : AbstractValidator<VerifyDeviceCommand>
{
    public VerifyDeviceCommandValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
        RuleFor(x => x.AttendanceDeviceId).NotEmpty();
    }
}

public class VerifyDeviceCommandHandler : IRequestHandler<VerifyDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public VerifyDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(VerifyDeviceCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);
            
        if (attendance == null)
            return Result.Failure(new[] { "Attendance not found." });
            
        attendance.MarkDeviceVerified(true);
        
        _context.Attendances.Update(attendance);

        return Result.Success();
    }
}

