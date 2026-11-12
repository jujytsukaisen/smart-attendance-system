using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Attendances.DTOs;

namespace SmartAttendance.Application.Features.Attendances.Queries;

/// <summary>
/// Returns the ordered event timeline for a specific attendance record.
/// Used by the UI to render the daily scan timeline:
/// e.g. 08:00 CheckIn → 10:15 BreakStart → 10:30 BreakEnd → 16:00 CheckOut.
/// </summary>
public class GetAttendanceEventsQuery : IRequest<Result<List<AttendanceEventDto>>>
{
    /// <summary>The attendance record to retrieve events for.</summary>
    public Guid AttendanceId { get; set; }
}

public class GetAttendanceEventsQueryHandler
    : IRequestHandler<GetAttendanceEventsQuery, Result<List<AttendanceEventDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AttendanceEventDto>>> Handle(
        GetAttendanceEventsQuery request,
        CancellationToken cancellationToken)
    {
        var events = await _context.AttendanceEvents
            .Where(e => e.AttendanceId == request.AttendanceId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        var dtos = events.Select(e => new AttendanceEventDto
        {
            Id           = e.Id,
            AttendanceId = e.AttendanceId,
            Timestamp    = e.Timestamp,
            EventType    = e.EventType,
            Note         = e.Note
        }).ToList();

        return Result<List<AttendanceEventDto>>.Success(dtos);
    }
}
