using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendances.Commands;

public class RestoreAttendanceCommand : IRequest<Result<Unit>>
{
    public Guid AttendanceId { get; set; }
}

public class RestoreAttendanceCommandHandler : IRequestHandler<RestoreAttendanceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public RestoreAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(RestoreAttendanceCommand request, CancellationToken cancellationToken)
    {
        // Using IgnoreQueryFilters to find soft-deleted entities
        var entity = await _context.Attendances
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure(new[] { "Attendance not found." });
        }

        // Restore logically depending on the base entity setup. Usually IsDeleted = false
        // The project has soft delete configured on BaseTenantEntity.
        entity.Restore();
        
        return Result<Unit>.Success(Unit.Value);
    }
}
