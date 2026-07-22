using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Holidays.Queries;

public class HolidayDetailsDto : IMapFrom<Holiday>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Holiday, HolidayDetailsDto>();
    }
}

public class GetHolidayByIdQuery : IRequest<Result<HolidayDetailsDto>>
{
    public Guid HolidayId { get; set; }
}

public class GetHolidayByIdQueryValidator : AbstractValidator<GetHolidayByIdQuery>
{
    public GetHolidayByIdQueryValidator()
    {
        RuleFor(x => x.HolidayId).NotEmpty();
    }
}

public class GetHolidayByIdQueryHandler : IRequestHandler<GetHolidayByIdQuery, Result<HolidayDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetHolidayByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<HolidayDetailsDto>> Handle(GetHolidayByIdQuery request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.HolidayId, cancellationToken);
            
        if (holiday == null) return Result<HolidayDetailsDto>.Failure(new[] { "Holiday not found." });
        
        var dto = _mapper.Map<HolidayDetailsDto>(holiday);
        
        return Result<HolidayDetailsDto>.Success(dto);
    }
}

