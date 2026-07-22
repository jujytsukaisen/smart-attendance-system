using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class CheckInResponse
{
    public Guid AttendanceId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CheckInCommand : IRequest<Result<CheckInResponse>>
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public Guid? AttendanceDeviceId { get; set; }
    public int CheckInMethod { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string DeviceInformation { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
}

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<CheckInResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFaceRecognitionService _faceRecognitionService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ICurrentUserService _currentUserService;

    public CheckInCommandHandler(
        IApplicationDbContext context, 
        IFaceRecognitionService faceRecognitionService,
        IGeoLocationService geoLocationService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _faceRecognitionService = faceRecognitionService;
        _geoLocationService = geoLocationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CheckInResponse>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<CheckInResponse>.Failure(new[] { "Tenant ID is missing or invalid." });
        }
        
        var attendance = SmartAttendance.Domain.Entities.Attendance.CreateCheckIn(
            companyId,
            request.EmployeeId,
            request.AttendanceDeviceId ?? Guid.NewGuid(),
            request.AttendancePolicyId,
            request.ShiftId,
            request.CheckInTime,
            (SmartAttendance.Domain.Enums.CheckInMethod)request.CheckInMethod,
            request.Latitude,
            request.Longitude,
            request.IPAddress,
            request.DeviceInformation);

        await _context.Attendances.AddAsync(attendance, cancellationToken);
        // SaveChangesAsync is handled by TransactionBehavior

        var response = new CheckInResponse
        {
            AttendanceId = attendance.Id,
            CheckInTime = request.CheckInTime,
            Status = "CheckedIn"
        };
        await Task.CompletedTask;
        return Result<CheckInResponse>.Success(response);
    }
}
