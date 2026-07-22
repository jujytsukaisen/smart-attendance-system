using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.AttendanceDevices.DTOs;

namespace SmartAttendance.Application.Features.AttendanceDevices.Queries;

public class GetAttendanceDevicesByCompanyQuery : IRequest<Result<List<AttendanceDeviceDto>>>
{
    public Guid CompanyId { get; set; }

    public GetAttendanceDevicesByCompanyQuery(Guid companyId)
    {
        CompanyId = companyId;
    }
}

public class GetAttendanceDevicesByCompanyQueryHandler : IRequestHandler<GetAttendanceDevicesByCompanyQuery, Result<List<AttendanceDeviceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendanceDevicesByCompanyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<AttendanceDeviceDto>>> Handle(GetAttendanceDevicesByCompanyQuery request, CancellationToken cancellationToken)
    {
        var devices = await _context.AttendanceDevices
            .AsNoTracking()
            .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<AttendanceDeviceDto>>(devices);
        return Result<List<AttendanceDeviceDto>>.Success(dtos);
    }
}
