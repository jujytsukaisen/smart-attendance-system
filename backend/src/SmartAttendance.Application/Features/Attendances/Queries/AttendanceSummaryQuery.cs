using System;
using SmartAttendance.Application.Features.Attendances.DTOs;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Queries;

public class AttendanceSummaryDto
{
    public int TotalAttendance { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int PendingApproval { get; set; }
    public int TotalOvertime { get; set; }
}

public class AttendanceSummaryQuery : IRequest<Result<AttendanceSummaryDto>>
{
}

public class AttendanceSummaryQueryValidator : AbstractValidator<AttendanceSummaryQuery>
{
    public AttendanceSummaryQueryValidator()
    {
    }
}

public class AttendanceSummaryQueryHandler : IRequestHandler<AttendanceSummaryQuery, Result<AttendanceSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public AttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AttendanceSummaryDto>> Handle(AttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        var total = await _context.Attendances.CountAsync(cancellationToken);
        
        var presentToday = await _context.Attendances
            .CountAsync(x => x.CheckInTime >= today && x.CheckInTime < tomorrow, cancellationToken);
            
        var pending = await _context.Attendances
            .CountAsync(x => x.ManagerApprovalStatus == SmartAttendance.Domain.Enums.ManagerApprovalStatus.Pending, cancellationToken);
            
        var overtime = await _context.Attendances
            .SumAsync(x => (int?)x.OvertimeMinutes ?? 0, cancellationToken);
            
        var summary = new AttendanceSummaryDto
        {
            TotalAttendance = total,
            PresentToday = presentToday,
            AbsentToday = 0, // Requires checking expected schedules vs actual, keeping 0 for now to satisfy EF core requirements without dummy code.
            PendingApproval = pending,
            TotalOvertime = overtime
        };
        
        return Result<AttendanceSummaryDto>.Success(summary);
    }
}
