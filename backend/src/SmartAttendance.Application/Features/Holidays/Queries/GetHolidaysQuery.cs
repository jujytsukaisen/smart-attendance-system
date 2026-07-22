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

public class HolidayListDto : IMapFrom<Holiday>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Holiday, HolidayListDto>();
    }
}

public class GetHolidaysQuery : PaginationRequest, IRequest<Result<PagedResult<HolidayListDto>>>
{
}

public class GetHolidaysQueryValidator : AbstractValidator<GetHolidaysQuery>
{
    public GetHolidaysQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, Result<PagedResult<HolidayListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetHolidaysQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<HolidayListDto>>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Holidays.AsNoTracking().AsQueryable();

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

