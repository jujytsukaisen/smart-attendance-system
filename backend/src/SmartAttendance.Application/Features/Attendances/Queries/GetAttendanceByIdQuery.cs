using System;
using SmartAttendance.Application.Features.Attendances.DTOs;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Attendances.DTOs;
using System.Linq;

namespace SmartAttendance.Application.Features.Attendances.Queries;

public class GetAttendanceByIdQuery : IRequest<Result<AttendanceDetailsDto>>
{
    public Guid AttendanceId { get; set; }
}

public class GetAttendanceByIdQueryHandler : IRequestHandler<GetAttendanceByIdQuery, Result<AttendanceDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendanceByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AttendanceDetailsDto>> Handle(GetAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<AttendanceDetailsDto>.Failure(new[] { "Tenant ID is required." });
        }

        var item = await _context.Attendances
            .Where(x => x.Id == request.AttendanceId && x.CompanyId == companyId && !x.IsDeleted)
            .AsNoTracking()
            .ProjectTo<AttendanceDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (item == null)
            return Result<AttendanceDetailsDto>.Failure(new[] { "Attendance not found." });
            
        return Result<AttendanceDetailsDto>.Success(item);
    }
}
