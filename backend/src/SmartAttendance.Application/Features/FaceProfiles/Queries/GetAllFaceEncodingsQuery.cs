using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Queries;

/// <summary>
/// Query to get all active face profiles with their encodings for kiosk auto-detection.
/// Used by the Face Recognition Kiosk to build a local FaceMatcher without
/// knowing the employee ID in advance.
/// </summary>
public class GetAllFaceEncodingsQuery : IRequest<Result<List<FaceEncodingDto>>>
{
}

public class FaceEncodingDto
{
    public System.Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public System.Guid AttendancePolicyId { get; set; }
    public string FaceEncoding { get; set; } = string.Empty; // JSON array of 128 floats
}

public class GetAllFaceEncodingsQueryHandler : IRequestHandler<GetAllFaceEncodingsQuery, Result<List<FaceEncodingDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllFaceEncodingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FaceEncodingDto>>> Handle(GetAllFaceEncodingsQuery request, CancellationToken cancellationToken)
    {
        var profiles = await (from fp in _context.FaceProfiles.AsNoTracking()
                              where fp.IsActive && fp.FaceEncoding != null
                              join e in _context.Employees.AsNoTracking().Where(e => !e.IsDeleted)
                              on fp.EmployeeId equals e.Id
                              select new FaceEncodingDto
                              {
                                  EmployeeId = e.Id,
                                  EmployeeName = e.FirstName + " " + e.LastName,
                                  AttendancePolicyId = e.AttendancePolicyId,
                                  FaceEncoding = fp.FaceEncoding!
                              }).ToListAsync(cancellationToken);

        return Result<List<FaceEncodingDto>>.Success(profiles);
    }
}
