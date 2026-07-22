using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class DeleteAttendanceCommand : IRequest<Result<Unit>>
{
    public Guid AttendanceId { get; set; }
}

public class DeleteAttendanceCommandHandler : IRequestHandler<DeleteAttendanceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteAttendanceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure(new[] { "Attendance not found." });
        }

        _context.Attendances.Remove(entity); // EF interceptors handle soft delete
        
        return Result<Unit>.Success(Unit.Value);
    }
}
