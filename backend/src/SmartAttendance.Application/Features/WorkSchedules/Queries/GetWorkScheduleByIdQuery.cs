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

public class GetWorkScheduleByIdQuery : IRequest<Result<WorkScheduleDetailsDto>>
{
    public Guid WorkScheduleId { get; set; }
}

public class GetWorkScheduleByIdQueryValidator : AbstractValidator<GetWorkScheduleByIdQuery>
{
    public GetWorkScheduleByIdQueryValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class GetWorkScheduleByIdQueryHandler : IRequestHandler<GetWorkScheduleByIdQuery, Result<WorkScheduleDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkScheduleByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<WorkScheduleDetailsDto>> Handle(GetWorkScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.WorkSchedules
            .AsNoTracking()
            .Where(w => w.Id == request.WorkScheduleId)
            .ProjectTo<WorkScheduleDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return Result<WorkScheduleDetailsDto>.Failure(new[] { "Work schedule not found." });

        return Result<WorkScheduleDetailsDto>.Success(dto);
    }
}
