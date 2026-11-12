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

namespace SmartAttendance.Application.Features.Reports.Queries;

public class GetLeaveReportQuery : IRequest<Result<List<LeaveReportDto>>>
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? Status { get; set; }
}

public class GetLeaveReportQueryHandler
    : IRequestHandler<GetLeaveReportQuery, Result<List<LeaveReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<LeaveReportDto>>> Handle(
        GetLeaveReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leaves
            .Include(l => l.Employee)
            .AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(l => l.StartDate >= request.StartDate.Value.ToDateTime(TimeOnly.MinValue));
        if (request.EndDate.HasValue)
            query = query.Where(l => l.EndDate <= request.EndDate.Value.ToDateTime(TimeOnly.MaxValue));
        if (request.EmployeeId.HasValue)
            query = query.Where(l => l.EmployeeId == request.EmployeeId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<Domain.Enums.LeaveStatus>(request.Status, true, out var statusEnum))
            query = query.Where(l => l.LeaveStatus == statusEnum);

        var leaves = await query
            .OrderByDescending(l => l.StartDate)
            .Select(l => new LeaveReportDto
            {
                Id = l.Id,
                EmployeeName = $"{l.Employee.FirstName} {l.Employee.LastName}",
                LeaveType = l.LeaveType.ToString(),
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Status = l.LeaveStatus.ToString(),
                TotalDays = l.TotalDays
            })
            .ToListAsync(cancellationToken);

        return Result<List<LeaveReportDto>>.Success(leaves);
    }
}