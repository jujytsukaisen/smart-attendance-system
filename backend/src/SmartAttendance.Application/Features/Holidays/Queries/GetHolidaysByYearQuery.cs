using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Holidays.Queries;

public class HolidayDto : IMapFrom<Holiday>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public bool IsRecurring { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Holiday, HolidayDto>();
    }
}

public class GetHolidaysByYearQuery : PaginationRequest, IRequest<Result<PagedResult<HolidayDto>>>
{
    public int Year { get; set; }
}

public class GetHolidaysByYearQueryValidator : AbstractValidator<GetHolidaysByYearQuery>
{
    public GetHolidaysByYearQueryValidator()
    {
        RuleFor(x => x.Year).GreaterThanOrEqualTo(2000);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetHolidaysByYearQueryHandler : IRequestHandler<GetHolidaysByYearQuery, Result<PagedResult<HolidayDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetHolidaysByYearQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<HolidayDto>>> Handle(GetHolidaysByYearQuery request, CancellationToken cancellationToken)
    {
        var startOfYear = new System.DateTime(request.Year, 1, 1);
        var endOfYear = new System.DateTime(request.Year + 1, 1, 1);
        
        var query = _context.Holidays
            .Where(x => x.StartDate >= startOfYear && x.StartDate < endOfYear)
            .AsNoTracking()
            .AsQueryable();

        var count = await query.CountAsync(cancellationToken);

        var holidays = await query
            .OrderBy(x => x.StartDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<HolidayDto>>(holidays);
        
        var result = new PagedResult<HolidayDto>
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

        return Result<PagedResult<HolidayDto>>.Success(result);
    }
}

