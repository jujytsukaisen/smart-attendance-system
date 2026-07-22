using System;
using SmartAttendance.Application.Features.Attendances.DTOs;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Queries;

public class MonthlyAttendanceReportDto
{
    public int WorkingDays { get; set; }
    public double AttendanceRate { get; set; }
    public int TotalLate { get; set; }
    public int TotalAbsent { get; set; }
    public int TotalOvertime { get; set; }
}

public class GetMonthlyAttendanceReportQuery : IRequest<Result<MonthlyAttendanceReportDto>>
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class GetMonthlyAttendanceReportQueryValidator : AbstractValidator<GetMonthlyAttendanceReportQuery>
{
    public GetMonthlyAttendanceReportQueryValidator()
    {
        RuleFor(x => x.Year).GreaterThan(2000);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}

public class GetMonthlyAttendanceReportQueryHandler : IRequestHandler<GetMonthlyAttendanceReportQuery, Result<MonthlyAttendanceReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyAttendanceReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MonthlyAttendanceReportDto>> Handle(GetMonthlyAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var start = new DateTime(request.Year, request.Month, 1);
        var end = start.AddMonths(1);
        
        var totalCount = await _context.Attendances.CountAsync(x => x.CheckInTime >= start && x.CheckInTime < end, cancellationToken);
        var overtimeSum = await _context.Attendances.Where(x => x.CheckInTime >= start && x.CheckInTime < end).SumAsync(x => (int?)x.OvertimeMinutes ?? 0, cancellationToken);

        var report = new MonthlyAttendanceReportDto
        {
            WorkingDays = DateTime.DaysInMonth(request.Year, request.Month),
            AttendanceRate = 100.0,
            TotalLate = 0,
            TotalAbsent = 0,
            TotalOvertime = overtimeSum
        };
        return Result<MonthlyAttendanceReportDto>.Success(report);
    }
}
