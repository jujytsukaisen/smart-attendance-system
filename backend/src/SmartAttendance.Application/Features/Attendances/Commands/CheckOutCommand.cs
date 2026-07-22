using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class CheckOutResponse
{
    public Guid AttendanceId { get; set; }
    public DateTime CheckOutTime { get; set; }
}

public class CheckOutCommand : IRequest<Result<CheckOutResponse>>
{
    public Guid AttendanceId { get; set; }
    public int CheckOutMethod { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string DeviceInformation { get; set; } = string.Empty;
    public DateTime CheckOutTime { get; set; }
}

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<CheckOutResponse>>
{
    private readonly IApplicationDbContext _context;

    public CheckOutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CheckOutResponse>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);

        if (attendance == null)
            return Result<CheckOutResponse>.Failure(new[] { "Attendance record not found." });

        attendance.RecordCheckOut(
            request.CheckOutTime,
            (SmartAttendance.Domain.Enums.CheckInMethod)request.CheckOutMethod,
            request.Latitude,
            request.Longitude,
            request.IPAddress,
            request.DeviceInformation);

        var response = new CheckOutResponse
        {
            AttendanceId = attendance.Id,
            CheckOutTime = request.CheckOutTime
        };

        return Result<CheckOutResponse>.Success(response);
    }
}
