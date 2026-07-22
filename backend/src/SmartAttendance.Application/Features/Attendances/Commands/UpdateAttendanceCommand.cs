using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class UpdateAttendanceCommand : IRequest<Result<Unit>>
{
    public Guid AttendanceId { get; set; }
    public DateTime? CheckOutTime { get; set; }
}

public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure(new[] { "Attendance not found." });
        }

        if (request.CheckOutTime.HasValue)
        {
            // Update checking out data
            entity.RecordCheckOut(request.CheckOutTime.Value, SmartAttendance.Domain.Enums.CheckInMethod.Manual);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
