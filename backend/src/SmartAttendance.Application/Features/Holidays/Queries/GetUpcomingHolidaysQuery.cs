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

public class GetUpcomingHolidaysQuery : PaginationRequest, IRequest<Result<PagedResult<HolidayListDto>>>
{
    public int DaysAhead { get; set; } = 30;
}

public class GetUpcomingHolidaysQueryValidator : AbstractValidator<GetUpcomingHolidaysQuery>
{
    public GetUpcomingHolidaysQueryValidator()
    {
        RuleFor(x => x.DaysAhead).GreaterThan(0).LessThanOrEqualTo(365);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetUpcomingHolidaysQueryHandler : IRequestHandler<GetUpcomingHolidaysQuery, Result<PagedResult<HolidayListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetUpcomingHolidaysQueryHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PagedResult<HolidayListDto>>> Handle(GetUpcomingHolidaysQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.UtcNow.Date;
        var query = _context.Holidays
            .Where(x => x.StartDate >= today && x.IsActive)
            .AsNoTracking()
            .AsQueryable();

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

