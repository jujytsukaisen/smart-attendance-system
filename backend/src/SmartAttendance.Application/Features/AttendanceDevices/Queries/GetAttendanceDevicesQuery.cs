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
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.AttendanceDevices.Queries;

public class GetAttendanceDevicesQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceDeviceListDto>>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public DeviceType? DeviceType { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetAttendanceDevicesQueryHandler : IRequestHandler<GetAttendanceDevicesQuery, Result<PagedResult<AttendanceDeviceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendanceDevicesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<AttendanceDeviceListDto>>> Handle(GetAttendanceDevicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AttendanceDevices
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.CompanyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == request.CompanyId.Value);
        }
        else if (_currentUserService.CompanyId != Guid.Empty)
        {
            query = query.Where(x => x.CompanyId == _currentUserService.CompanyId);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(x => x.BranchId == request.BranchId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.DeviceType.HasValue)
        {
            query = query.Where(x => x.DeviceType == request.DeviceType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.Name.ToLower().Contains(searchTerm) || 
                x.Code.ToLower().Contains(searchTerm) || 
                (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "code" => request.SortDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "status" => request.SortDesc ? query.OrderByDescending(x => x.DeviceStatus) : query.OrderBy(x => x.DeviceStatus),
            _ => request.SortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var deviceEntities = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var devices = _mapper.Map<List<AttendanceDeviceListDto>>(deviceEntities);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var result = new PagedResult<AttendanceDeviceListDto>
        {
            Items = devices,
            Pagination = new PaginationResponse
            {
                PageNumber = request.PageNumber,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = request.PageNumber < totalPages,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<AttendanceDeviceListDto>>.Success(result);
    }
}
