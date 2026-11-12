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
/// DTO returned by <see cref="GetCurrentEmployeeStateQuery"/>.
/// </summary>
public class EmployeeCurrentStateDto
{
    /// <summary>The employee identifier.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// The live attendance state for today.
    /// Values: "NotCheckedIn" | "Working" | "OnBreak" | "Finished".
    /// </summary>
    public string State { get; set; } = "NotCheckedIn";

    /// <summary>Time of the first CheckIn today (null if not yet checked in).</summary>
    public DateTime? CheckInTime { get; set; }

    /// <summary>Timestamp of the most recent event (null if not yet checked in).</summary>
    public DateTime? LastEventTime { get; set; }

    /// <summary>The parent attendance record identifier (null if not yet checked in).</summary>
    public Guid? AttendanceId { get; set; }
}

/// <summary>
/// Returns the live attendance state of an employee for the current workday.
/// Used by the dashboard and employee-facing UI to display the current status badge.
/// </summary>
public class GetCurrentEmployeeStateQuery : IRequest<Result<EmployeeCurrentStateDto>>
{
    /// <summary>The employee whose current state is being queried.</summary>
    public Guid EmployeeId { get; set; }
}

public class GetCurrentEmployeeStateQueryHandler
    : IRequestHandler<GetCurrentEmployeeStateQuery, Result<EmployeeCurrentStateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentEmployeeStateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EmployeeCurrentStateDto>> Handle(
        GetCurrentEmployeeStateQuery request,
        CancellationToken cancellationToken)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1).AddTicks(-1);

        // Load today's attendance record with events (if any)
        var attendance = await _context.Attendances
            .Include(a => a.Events)
            .FirstOrDefaultAsync(
                a => a.EmployeeId == request.EmployeeId
                  && a.CheckInTime >= todayStart
                  && a.CheckInTime <= todayEnd,
                cancellationToken);

        if (attendance is null)
        {
            return Result<EmployeeCurrentStateDto>.Success(new EmployeeCurrentStateDto
            {
                EmployeeId = request.EmployeeId,
                State      = EmployeeAttendanceState.NotCheckedIn.ToString()
            });
        }

        var currentState = attendance.CheckOutTime.HasValue
            ? EmployeeAttendanceState.Finished
            : (!attendance.Events.Any()
                ? EmployeeAttendanceState.Working
                : AttendanceStateHelper.ComputeState(attendance.Events, attendance.CheckOutTime));
        var lastEvent    = attendance.Events.OrderBy(e => e.Timestamp).LastOrDefault();


        return Result<EmployeeCurrentStateDto>.Success(new EmployeeCurrentStateDto
        {
            EmployeeId    = request.EmployeeId,
            State         = currentState.ToString(),
            CheckInTime   = attendance.CheckInTime,
            LastEventTime = lastEvent?.Timestamp,
            AttendanceId  = attendance.Id
        });
    }
}
