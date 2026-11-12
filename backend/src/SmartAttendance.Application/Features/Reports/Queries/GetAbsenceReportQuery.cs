using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Reports.DTOs;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Reports.Queries;

public class GetAbsenceReportQuery : IRequest<Result<List<AbsenceReportDto>>>
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class GetAbsenceReportQueryHandler
    : IRequestHandler<GetAbsenceReportQuery, Result<List<AbsenceReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAbsenceReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AbsenceReportDto>>> Handle(
        GetAbsenceReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var endDate = request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var employeesQuery = _context.Employees
            .Where(e => e.EmployeeStatus == EmployeeStatus.Active)
            .Include(e => e.Department)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            employeesQuery = employeesQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);
        if (request.EmployeeId.HasValue)
            employeesQuery = employeesQuery.Where(e => e.Id == request.EmployeeId.Value);

        var employees = await employeesQuery.ToListAsync(cancellationToken);

        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        var attendances = await _context.Attendances
            .Where(a => a.CheckInTime >= startDateTime && a.CheckInTime <= endDateTime)
            .ToListAsync(cancellationToken);

        var totalWorkingDays = endDate.DayNumber - startDate.DayNumber + 1;

        var result = employees
            .Select(e =>
            {
                var attendedDays = attendances.Count(a => a.EmployeeId == e.Id);
                var absentDays = Math.Max(0, totalWorkingDays - attendedDays);
                return new AbsenceReportDto
                {
                    Id = e.Id,
                    EmployeeName = $"{e.FirstName} {e.LastName}",
                    Department = e.Department?.Name ?? string.Empty,
                    AbsentDaysCount = absentDays
                };
            })
            .Where(x => x.AbsentDaysCount > 0)
            .OrderByDescending(x => x.AbsentDaysCount)
            .ToList();

        return Result<List<AbsenceReportDto>>.Success(result);
    }
}