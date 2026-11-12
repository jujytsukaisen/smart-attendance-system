using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Helpers;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.Queries;

/// <summary>
/// Returns the live attendance state of the currently authenticated employee for today.
/// Auto-resolves the employee via ICurrentUserService — no employeeId required.
/// </summary>
public class GetMyEmployeeStateQuery : IRequest<Result<EmployeeCurrentStateDto>>
{
}

public class GetMyEmployeeStateQueryHandler
    : IRequestHandler<GetMyEmployeeStateQuery, Result<EmployeeCurrentStateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyEmployeeStateQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<EmployeeCurrentStateDto>> Handle(
        GetMyEmployeeStateQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var domainUserId))
        {
            return Result<EmployeeCurrentStateDto>.Failure(new[] { "User ID is required." });
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == domainUserId && !e.IsDeleted, cancellationToken);

        if (employee == null)
        {
            return Result<EmployeeCurrentStateDto>.Success(new EmployeeCurrentStateDto
            {
                EmployeeId = Guid.Empty,
                State = EmployeeAttendanceState.NotCheckedIn.ToString()
            });
        }

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1).AddTicks(-1);

        var attendance = await _context.Attendances
            .Include(a => a.Events)
            .FirstOrDefaultAsync(
                a => a.EmployeeId == employee.Id
                  && a.CheckInTime >= todayStart
                  && a.CheckInTime <= todayEnd,
                cancellationToken);

        if (attendance is null)
        {
            return Result<EmployeeCurrentStateDto>.Success(new EmployeeCurrentStateDto
            {
                EmployeeId = employee.Id,
                State      = EmployeeAttendanceState.NotCheckedIn.ToString()
            });
        }

        var currentState = attendance.CheckOutTime.HasValue
            ? EmployeeAttendanceState.Finished
            : (!attendance.Events.Any()
                ? EmployeeAttendanceState.Working
                : AttendanceStateHelper.ComputeState(attendance.Events, attendance.CheckOutTime));

        var lastEvent = attendance.Events.OrderBy(e => e.Timestamp).LastOrDefault();

        return Result<EmployeeCurrentStateDto>.Success(new EmployeeCurrentStateDto
        {
            EmployeeId    = employee.Id,
            State         = currentState.ToString(),
            CheckInTime   = attendance.CheckInTime,
            LastEventTime = lastEvent?.Timestamp,
            AttendanceId  = attendance.Id
        });
    }
}
