using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Shifts.DTOs;

namespace SmartAttendance.Application.Features.Shifts.Queries;

public class GetShiftsByWorkScheduleQuery : PaginationRequest, IRequest<Result<PagedResult<ShiftListDto>>>
{
    public Guid WorkScheduleId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetShiftsByWorkScheduleQueryValidator : AbstractValidator<GetShiftsByWorkScheduleQuery>
{
    public GetShiftsByWorkScheduleQueryValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetShiftsByWorkScheduleQueryHandler : IRequestHandler<GetShiftsByWorkScheduleQuery, Result<PagedResult<ShiftListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetShiftsByWorkScheduleQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ShiftListDto>>> Handle(GetShiftsByWorkScheduleQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Shifts
            .AsNoTracking()
            .Where(s => s.WorkScheduleId == request.WorkScheduleId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Name.Contains(request.SearchTerm) || x.Code.Contains(request.SearchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "code" => request.SortDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "starttime" => request.SortDesc ? query.OrderByDescending(x => x.StartTime) : query.OrderBy(x => x.StartTime),
            _ => request.SortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ShiftListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ShiftListDto>
        {
            Items = items,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<ShiftListDto>>.Success(result);
    }
}
