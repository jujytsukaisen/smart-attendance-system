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
using SmartAttendance.Application.Features.WorkSchedules.DTOs;

namespace SmartAttendance.Application.Features.WorkSchedules.Queries;

public class GetWorkSchedulesQuery : PaginationRequest, IRequest<Result<PagedResult<WorkScheduleListDto>>>
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
    public bool? IsActive { get; set; }
}

public class GetWorkSchedulesQueryValidator : AbstractValidator<GetWorkSchedulesQuery>
{
    public GetWorkSchedulesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetWorkSchedulesQueryHandler : IRequestHandler<GetWorkSchedulesQuery, Result<PagedResult<WorkScheduleListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkSchedulesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<WorkScheduleListDto>>> Handle(GetWorkSchedulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkSchedules.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var cleanSearchTerm = request.SearchTerm.Trim();
            query = query.Where(w => w.Name.Contains(cleanSearchTerm) || 
                                     (w.Description != null && w.Description.Contains(cleanSearchTerm)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(w => w.IsActive == request.IsActive.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "weeklyworkinghours" => request.SortDesc ? query.OrderByDescending(x => x.WeeklyWorkingHours) : query.OrderBy(x => x.WeeklyWorkingHours),
            _ => request.SortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<WorkScheduleListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<WorkScheduleListDto>
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

        return Result<PagedResult<WorkScheduleListDto>>.Success(result);
    }
}
