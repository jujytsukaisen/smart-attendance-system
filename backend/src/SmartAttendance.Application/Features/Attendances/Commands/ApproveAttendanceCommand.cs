using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class ApproveAttendanceCommand : IRequest<Result<Unit>>
{
    public Guid AttendanceId { get; set; }
}

public class ApproveAttendanceCommandHandler : IRequestHandler<ApproveAttendanceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ApproveAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ApproveAttendanceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);

        if (entity == null)
            return Result<Unit>.Failure(new[] { "Attendance not found." });

        entity.Approve();
        
        return Result<Unit>.Success(Unit.Value);
    }
}
