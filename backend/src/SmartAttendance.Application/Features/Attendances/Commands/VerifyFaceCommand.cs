using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Commands;

public class VerifyFaceCommand : IRequest<Result>
{
    public Guid AttendanceId { get; set; }
    public Guid EmployeeId { get; set; }
}

public class VerifyFaceCommandValidator : AbstractValidator<VerifyFaceCommand>
{
    public VerifyFaceCommandValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class VerifyFaceCommandHandler : IRequestHandler<VerifyFaceCommand, Result>
{
    private readonly IFaceRecognitionService _faceRecognitionService;
    private readonly IApplicationDbContext _context;

    public VerifyFaceCommandHandler(IFaceRecognitionService faceRecognitionService, IApplicationDbContext context)
    {
        _faceRecognitionService = faceRecognitionService;
        _context = context;
    }

    public async Task<Result> Handle(VerifyFaceCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);
            
        if (attendance == null)
            return Result.Failure(new[] { "Attendance not found." });
            
        attendance.MarkFaceVerified(true);
        
        _context.Attendances.Update(attendance);

        return Result.Success();
    }
}

