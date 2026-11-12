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

namespace SmartAttendance.Application.Features.Attendances.Queries;

public class GetAttendancesByCompanyQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceListDto>>>
{
    public Guid CompanyId { get; set; }
}

public class GetAttendancesByCompanyQueryHandler : IRequestHandler<GetAttendancesByCompanyQuery, Result<PagedResult<AttendanceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendancesByCompanyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AttendanceListDto>>> Handle(GetAttendancesByCompanyQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Attendances
            .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
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

