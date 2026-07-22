using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.AttendanceDevices.DTOs;

namespace SmartAttendance.Application.Features.AttendanceDevices.Queries;

public class GetAttendanceDeviceByIdQuery : IRequest<Result<AttendanceDeviceDetailsDto>>
{
    public Guid Id { get; set; }

    public GetAttendanceDeviceByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetAttendanceDeviceByIdQueryHandler : IRequestHandler<GetAttendanceDeviceByIdQuery, Result<AttendanceDeviceDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendanceDeviceByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AttendanceDeviceDetailsDto>> Handle(GetAttendanceDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.AttendanceDevices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (device == null)
            return Result<AttendanceDeviceDetailsDto>.Failure(new[] { "Attendance device not found." });

        var dto = _mapper.Map<AttendanceDeviceDetailsDto>(device);
        return Result<AttendanceDeviceDetailsDto>.Success(dto);
    }
}
