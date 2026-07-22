using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.AttendanceDevices.Commands;

public class DeactivateAttendanceDeviceCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }

    public DeactivateAttendanceDeviceCommand(Guid id)
    {
        Id = id;
    }
}

public class DeactivateAttendanceDeviceCommandHandler : IRequestHandler<DeactivateAttendanceDeviceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeactivateAttendanceDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeactivateAttendanceDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.AttendanceDevices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device == null)
            return Result<Unit>.Failure(new[] { "Attendance device not found." });

        device.Deactivate();

        return Result<Unit>.Success(Unit.Value);
    }
}
