using FluentValidation;
using System;
using SmartAttendance.Application.Features.Attendances.DTOs;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Queries;

public class SearchAttendancesQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceListDto>>>
{
    public Guid? EmployeeId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string AttendanceStatus { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchAttendancesQueryValidator : AbstractValidator<SearchAttendancesQuery>
{
    public SearchAttendancesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be after or equal to StartDate.");
    }
}

public class SearchAttendancesQueryHandler : IRequestHandler<SearchAttendancesQuery, Result<PagedResult<AttendanceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchAttendancesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AttendanceListDto>>> Handle(SearchAttendancesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Attendances.AsNoTracking().AsQueryable();

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CheckInTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CheckInTime <= request.EndDate.Value);
        }

        var count = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CheckInTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AttendanceListDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                CheckInTime = x.CheckInTime,
                CheckOutTime = x.CheckOutTime,
                AttendanceStatus = x.AttendanceStatus
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AttendanceListDto>
        {
            Items = items,
            Pagination = new PaginationResponse { TotalCount = count, PageNumber = request.PageNumber, TotalPages = (int)Math.Ceiling(count / (double)request.PageSize), HasPreviousPage = request.PageNumber > 1, HasNextPage = request.PageNumber < (int)Math.Ceiling(count / (double)request.PageSize) }
        };

        return Result<PagedResult<AttendanceListDto>>.Success(result);
    }
}


