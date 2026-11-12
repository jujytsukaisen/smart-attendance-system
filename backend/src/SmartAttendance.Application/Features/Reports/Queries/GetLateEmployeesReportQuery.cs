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

public class GetLateEmployeesReportQuery : IRequest<Result<List<LateEmployeesReportDto>>>
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class GetLateEmployeesReportQueryHandler
    : IRequestHandler<GetLateEmployeesReportQuery, Result<List<LateEmployeesReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetLateEmployeesReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<LateEmployeesReportDto>>> Handle(
        GetLateEmployeesReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = (request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))).ToDateTime(TimeOnly.MinValue);
        var endDate = (request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToDateTime(TimeOnly.MaxValue);

        var query = _context.Attendances
            .Where(a => a.AttendanceStatus == AttendanceStatus.Late
                        && a.CheckInTime >= startDate && a.CheckInTime <= endDate)
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == request.DepartmentId.Value);
        if (request.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeId.Value);

        var attendances = await query.ToListAsync(cancellationToken);

        var result = attendances
            .GroupBy(a => a.EmployeeId)
            .Select(g => new LateEmployeesReportDto
            {
                Id = g.Key,
                EmployeeName = $"{g.First().Employee.FirstName} {g.First().Employee.LastName}",
                Department = g.First().Employee.Department?.Name ?? string.Empty,
                TotalLateMinutes = g.Sum(a => a.LateMinutes),
                LateDaysCount = g.Count()
            })
            .OrderByDescending(x => x.TotalLateMinutes)
            .ToList();

        return Result<List<LateEmployeesReportDto>>.Success(result);
    }
}