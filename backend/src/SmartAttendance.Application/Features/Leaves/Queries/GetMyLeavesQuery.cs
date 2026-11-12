using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Leaves.Queries;

namespace SmartAttendance.Application.Features.Leaves.Queries;

/// <summary>
/// Returns only the current employee's own leave records.
/// </summary>
public class GetMyLeavesQuery : PaginationRequest, IRequest<Result<PagedResult<LeaveListDto>>>
{
}

public class GetMyLeavesQueryHandler : IRequestHandler<GetMyLeavesQuery, Result<PagedResult<LeaveListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetMyLeavesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<LeaveListDto>>> Handle(GetMyLeavesQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var domainUserId))
        {
            return Result<PagedResult<LeaveListDto>>.Failure(new[] { "User ID is required." });
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == domainUserId && !e.IsDeleted, cancellationToken);

        if (employee == null)
        {
            return Result<PagedResult<LeaveListDto>>.Success(new PagedResult<LeaveListDto>
            {
                Items = new System.Collections.Generic.List<LeaveListDto>(),
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

        var baseQuery = _context.Leaves
            .Where(l => l.EmployeeId == employee.Id && !l.IsDeleted)
            .Include(l => l.Employee)
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var leaves = await baseQuery
            .OrderByDescending(l => l.StartDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<System.Collections.Generic.List<LeaveListDto>>(leaves);

        var result = new PagedResult<LeaveListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };

        return Result<PagedResult<LeaveListDto>>.Success(result);
    }
}
