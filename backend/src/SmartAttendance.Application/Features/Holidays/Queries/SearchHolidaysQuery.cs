using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Holidays.Queries;

public class SearchHolidaysQuery : PaginationRequest, IRequest<Result<PagedResult<HolidayListDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchHolidaysQueryValidator : AbstractValidator<SearchHolidaysQuery>
{
    public SearchHolidaysQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class SearchHolidaysQueryHandler : IRequestHandler<SearchHolidaysQuery, Result<PagedResult<HolidayListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchHolidaysQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<HolidayListDto>>> Handle(SearchHolidaysQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Holidays.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Name.Contains(request.SearchTerm) || (x.Description != null && x.Description.Contains(request.SearchTerm)));
        }

        var count = await query.CountAsync(cancellationToken);

        var holidays = await query
            .OrderBy(x => x.StartDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<HolidayListDto>>(holidays);
        
        var result = new PagedResult<HolidayListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse 
            {
                TotalCount = count,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(count / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)System.Math.Ceiling(count / (double)request.PageSize)
            }
        };

        return Result<PagedResult<HolidayListDto>>.Success(result);
    }
}

