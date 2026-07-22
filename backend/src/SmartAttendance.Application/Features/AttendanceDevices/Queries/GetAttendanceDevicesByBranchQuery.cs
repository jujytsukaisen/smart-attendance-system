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

public class GetAttendanceDevicesByBranchQuery : IRequest<Result<List<AttendanceDeviceDto>>>
{
    public Guid BranchId { get; set; }

    public GetAttendanceDevicesByBranchQuery(Guid branchId)
    {
        BranchId = branchId;
    }
}

public class GetAttendanceDevicesByBranchQueryHandler : IRequestHandler<GetAttendanceDevicesByBranchQuery, Result<List<AttendanceDeviceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendanceDevicesByBranchQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<AttendanceDeviceDto>>> Handle(GetAttendanceDevicesByBranchQuery request, CancellationToken cancellationToken)
    {
        var devices = await _context.AttendanceDevices
            .AsNoTracking()
            .Where(x => x.BranchId == request.BranchId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<AttendanceDeviceDto>>(devices);
        return Result<List<AttendanceDeviceDto>>.Success(dtos);
    }
}
