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

public class GetAttendancesQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceListDto>>>
{
    public Guid? EmployeeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetAttendancesQueryHandler : IRequestHandler<GetAttendancesQuery, Result<PagedResult<AttendanceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendancesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<AttendanceListDto>>> Handle(GetAttendancesQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<PagedResult<AttendanceListDto>>.Failure(new[] { "Tenant ID is required." });
        }

        var query = _context.Attendances
            .Where(x => x.CompanyId == companyId && !x.IsDeleted)
            .AsNoTracking();

        if (Guid.TryParse(_currentUserService.UserId, out var currentUserId))
        {
            var userRoles = await _context.Users
                .Where(u => u.Id == currentUserId)
                .SelectMany(u => u.Roles.Select(r => r.Name))
                .ToListAsync(cancellationToken);

            bool isEmployeeOnly = userRoles.Count > 0 && userRoles.All(r => r == "موظف" || r == "Employee");
            if (isEmployeeOnly)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == currentUserId, cancellationToken);
                if (employee != null)
                {
                    query = query.Where(x => x.EmployeeId == employee.Id);
                }
                else
                {
                    // User has employee role but no employee record yet
                    query = query.Where(x => false);
                }
            }
        }

        if (request.EmployeeId.HasValue)
            query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);

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

