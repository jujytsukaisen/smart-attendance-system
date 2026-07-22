using System;
using SmartAttendance.Application.Features.Attendances.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Attendances.DTOs;

namespace SmartAttendance.Application.Features.Attendances.Queries;

public class GetAttendancesByBranchQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceListDto>>>
{
    public Guid BranchId { get; set; }
}

public class GetAttendancesByBranchQueryHandler : IRequestHandler<GetAttendancesByBranchQuery, Result<PagedResult<AttendanceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendancesByBranchQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<AttendanceListDto>>> Handle(GetAttendancesByBranchQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<PagedResult<AttendanceListDto>>.Failure(new[] { "Tenant ID is required." });
        }

        var query = _context.Attendances
            .Include(a => a.Employee)
            .Where(x => x.CompanyId == companyId && x.Employee.BranchId == request.BranchId && !x.IsDeleted)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CheckInTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<AttendanceListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AttendanceListDto>
        {
            Items = items,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };

        return Result<PagedResult<AttendanceListDto>>.Success(result);
    }
}

