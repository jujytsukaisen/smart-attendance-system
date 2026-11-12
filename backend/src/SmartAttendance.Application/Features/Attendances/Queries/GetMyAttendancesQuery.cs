using System;
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

/// <summary>
/// Returns only the current employee's own attendance records.
/// Resolves the Employee via ICurrentUserService.UserId → Employee.UserId FK.
/// </summary>
public class GetMyAttendancesQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceListDto>>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetMyAttendancesQueryHandler : IRequestHandler<GetMyAttendancesQuery, Result<PagedResult<AttendanceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAttendancesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<AttendanceListDto>>> Handle(GetMyAttendancesQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var domainUserId))
        {
            return Result<PagedResult<AttendanceListDto>>.Failure(new[] { "User ID is required." });
        }

        // Find the employee linked to this domain user
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == domainUserId && !e.IsDeleted, cancellationToken);

        if (employee == null)
        {
            // Return empty result instead of error — user may not be linked to an employee yet
            return Result<PagedResult<AttendanceListDto>>.Success(new PagedResult<AttendanceListDto>
            {
                Items = new System.Collections.Generic.List<AttendanceListDto>(),
                Pagination = new PaginationResponse
                {
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false
                }
            });
        }

        var query = _context.Attendances
            .Where(x => x.EmployeeId == employee.Id && !x.IsDeleted)
            .AsNoTracking();

        if (request.StartDate.HasValue)
            query = query.Where(x => x.CheckInTime.Date >= request.StartDate.Value.Date);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.CheckInTime.Date <= request.EndDate.Value.Date);

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
