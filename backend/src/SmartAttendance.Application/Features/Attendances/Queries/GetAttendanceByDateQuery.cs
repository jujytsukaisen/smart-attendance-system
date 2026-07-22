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

public class GetAttendanceByDateQuery : PaginationRequest, IRequest<Result<PagedResult<AttendanceListDto>>>
{
    public DateTime Date { get; set; }
}

public class GetAttendanceByDateQueryValidator : AbstractValidator<GetAttendanceByDateQuery>
{
    public GetAttendanceByDateQueryValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetAttendanceByDateQueryHandler : IRequestHandler<GetAttendanceByDateQuery, Result<PagedResult<AttendanceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendanceByDateQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AttendanceListDto>>> Handle(GetAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Attendances
            .Where(x => x.CheckInTime.Date == request.Date.Date)
            .AsNoTracking()
            .AsQueryable();

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


