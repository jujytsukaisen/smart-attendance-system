using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class CreateAttendanceCommand : IRequest<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public Guid AttendanceDeviceId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime CheckInTime { get; set; }
    public CheckInMethod CheckInMethod { get; set; }
}

public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateAttendanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<Guid>.Failure(new[] { "Tenant ID is required." });
        }

        var entity = SmartAttendance.Domain.Entities.Attendance.CreateCheckIn(
            companyId,
            request.EmployeeId,
            request.AttendanceDeviceId,
            request.AttendancePolicyId,
            request.ShiftId,
            request.CheckInTime,
            request.CheckInMethod
        );

        await _context.Attendances.AddAsync(entity, cancellationToken);
        // SaveChangesAsync is handled by TransactionBehavior

        await Task.CompletedTask;
        return Result<Guid>.Success(entity.Id);
    }
}
