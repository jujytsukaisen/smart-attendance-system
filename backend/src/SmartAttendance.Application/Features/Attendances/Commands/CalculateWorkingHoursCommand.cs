using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Commands;

public class WorkingHoursDto
{
    public int WorkingMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
}

public class CalculateWorkingHoursCommand : IRequest<Result<WorkingHoursDto>>
{
    public Guid AttendanceId { get; set; }
}

public class CalculateWorkingHoursCommandValidator : AbstractValidator<CalculateWorkingHoursCommand>
{
    public CalculateWorkingHoursCommandValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
    }
}

public class CalculateWorkingHoursCommandHandler : IRequestHandler<CalculateWorkingHoursCommand, Result<WorkingHoursDto>>
{
    private readonly IApplicationDbContext _context;

    public CalculateWorkingHoursCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WorkingHoursDto>> Handle(CalculateWorkingHoursCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances.FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);
        if (attendance == null) return Result<WorkingHoursDto>.Failure(new[] { "Attendance not found." });
        
        attendance.CalculateWorkingHours(0, 0, 0, 0);
        
        _context.Attendances.Update(attendance);

        var dto = new WorkingHoursDto
        {
            WorkingMinutes = attendance.WorkingMinutes,
            OvertimeMinutes = attendance.OvertimeMinutes
        };
        return Result<WorkingHoursDto>.Success(dto);
    }
}

